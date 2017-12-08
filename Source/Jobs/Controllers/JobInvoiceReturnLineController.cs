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
using JobInvoiceReturnDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobInvoiceReturnLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        private bool EventException = false;

        IJobInvoiceReturnLineService _JobInvoiceReturnLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public JobInvoiceReturnLineController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceReturnLineService = new JobInvoiceReturnLineService(db);
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _JobInvoiceReturnLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForInvoice(int id, int sid)
        {
            JobInvoiceReturnLineFilterViewModel vm = new JobInvoiceReturnLineFilterViewModel();
            vm.JobInvoiceReturnHeaderId = id;
            vm.JobWorkerId = sid;
            JobInvoiceReturnHeader Header = new JobInvoiceReturnHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForReceipt(int id, int sid)
        {
            JobInvoiceReturnLineFilterViewModel vm = new JobInvoiceReturnLineFilterViewModel();
            vm.JobInvoiceReturnHeaderId = id;
            vm.JobWorkerId = sid;
            JobInvoiceReturnHeader Header = new JobInvoiceReturnHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_ReceiptFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobInvoiceReturnLineFilterViewModel vm)
        {
            List<JobInvoiceReturnLineViewModel> temp = _JobInvoiceReturnLineService.GetJobInvoiceForFilters(vm).ToList();
            var InvoiceReturn = new JobInvoiceReturnHeaderService(db).Find(vm.JobInvoiceReturnHeaderId);
            var GoodsReturn = db.JobReturnHeader.Find(InvoiceReturn.JobReturnHeaderId);

            if(!temp.Where(m=>m.ProductUidId==null).Any())
            {
                if (temp.Count() > 0)
                {
                    List<JobInvoiceReturnBarCodeSequenceViewModel> Sequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
                    List<JobInvoiceReturnBarCodeSequenceViewModel> GSequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
                    foreach (var item in temp)
                        Sequence.Add(new JobInvoiceReturnBarCodeSequenceViewModel
                        {
                            JobInvoiceReturnHeaderId = item.JobInvoiceReturnHeaderId,
                            JobInvoiceLineId = item.JobInvoiceLineId,
                            ProductName = item.ProductName,
                            Qty = item.Qty,
                            BalanceQty = item.InvoiceBalQty,
                        });


                    GSequence = (from p in Sequence
                                 orderby p.ProductName
                                 group p by p.ProductName into g
                                 select new JobInvoiceReturnBarCodeSequenceViewModel
                                 {
                                     JobInvoiceReturnHeaderId = g.Max(m => m.JobInvoiceReturnHeaderId),
                                     ProductName = g.Key,
                                     SJobInvLineIds = string.Join(",", g.Select(m => m.JobInvoiceLineId).ToList()),
                                     Qty = g.Sum(m => m.Qty),
                                     BalanceQty = g.Sum(m => m.BalanceQty),
                                 }).ToList();


                    JobInvoiceReturnBarCodeSequenceListViewModel SquenceList = new JobInvoiceReturnBarCodeSequenceListViewModel();
                    SquenceList.JobInvoiceReturnBarCodeSequenceViewModel = GSequence;
                    SquenceList.GodownId = GoodsReturn.GodownId;
                    return PartialView("_Sequence", SquenceList);

                }
            }


            JobInvoiceReturnMasterDetailModel svm = new JobInvoiceReturnMasterDetailModel();
            svm.JobInvoiceReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostReceipts(JobInvoiceReturnLineFilterViewModel vm)
        {
            List<JobInvoiceReturnLineViewModel> temp = _JobInvoiceReturnLineService.GetJobReceiveForFilters(vm).ToList();

            var InvoiceReturn = new JobInvoiceReturnHeaderService(db).Find(vm.JobInvoiceReturnHeaderId);
            var GoodsReturn = db.JobReturnHeader.Find(InvoiceReturn.JobReturnHeaderId);

            if (!temp.Where(m => m.ProductUidId == null).Any())
            {
                if (temp.Count() > 0)
                {
                    List<JobInvoiceReturnBarCodeSequenceViewModel> Sequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
                    List<JobInvoiceReturnBarCodeSequenceViewModel> GSequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
                    foreach (var item in temp)
                        Sequence.Add(new JobInvoiceReturnBarCodeSequenceViewModel
                        {
                            JobInvoiceReturnHeaderId = item.JobInvoiceReturnHeaderId,
                            JobInvoiceLineId = item.JobInvoiceLineId,
                            ProductName = item.ProductName,
                            Qty = item.Qty,
                            BalanceQty = item.InvoiceBalQty,
                        });


                    GSequence = (from p in Sequence
                                 orderby p.ProductName
                                 group p by p.ProductName into g
                                 select new JobInvoiceReturnBarCodeSequenceViewModel
                                 {
                                     JobInvoiceReturnHeaderId = g.Max(m => m.JobInvoiceReturnHeaderId),
                                     ProductName = g.Key,
                                     SJobInvLineIds = string.Join(",", g.Select(m => m.JobInvoiceLineId).ToList()),
                                     Qty = g.Sum(m => m.Qty),
                                     BalanceQty = g.Sum(m => m.BalanceQty),
                                 }).ToList();


                    JobInvoiceReturnBarCodeSequenceListViewModel SquenceList = new JobInvoiceReturnBarCodeSequenceListViewModel();
                    SquenceList.JobInvoiceReturnBarCodeSequenceViewModel = GSequence;
                    SquenceList.GodownId = GoodsReturn.GodownId;
                    return PartialView("_Sequence", SquenceList);

                }
            }


            JobInvoiceReturnMasterDetailModel svm = new JobInvoiceReturnMasterDetailModel();
            svm.JobInvoiceReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobInvoiceReturnMasterDetailModel vm)
        {
            int Cnt = 0;

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            Dictionary<int, decimal> LineStatusWeights = new Dictionary<int, decimal>();
            List<LineReferenceIds> RefIds = new List<LineReferenceIds>();
            List<LineChargeRates> LineChargeRates = new List<LineChargeRates>();

            List<JobInvoiceReturnLineViewModel> BarCodeBased = new List<JobInvoiceReturnLineViewModel>();

            int pk = 0;
            int Gpk = 0;
            int Serial = _JobInvoiceReturnLineService.GetMaxSr(vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId);
            bool HeaderChargeEdit = false;

            JobInvoiceReturnHeader Header = new JobInvoiceReturnHeaderService(db).Find(vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId);

            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new JobInvoiceReturnLineChargeService(db).GetMaxProductCharge(Header.JobInvoiceReturnHeaderId, "Web.JobInvoiceReturnLines", "JobInvoiceReturnHeaderId", "JobInvoiceReturnLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId.Value;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            JobReturnHeader GoodsRetHeader = new JobReturnHeaderService(_unitOfWork).Find(Header.JobReturnHeaderId ?? 0);

            bool BeforeSave = true;

            try
            {
                BeforeSave = JobInvoiceReturnDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {


                var InvLineIds = vm.JobInvoiceReturnLineViewModel.Select(m => m.JobInvoiceLineId).ToArray();
                var InvBalRecords = (from p in db.ViewJobInvoiceBalance
                                     where InvLineIds.Contains(p.JobInvoiceLineId)
                                     select new
                                     {
                                         LineId = p.JobInvoiceLineId,
                                         BalQty = p.BalanceQty,
                                     }).ToList();

                var InvoiceRecords = (from p in db.JobInvoiceLine.AsNoTracking()
                                      where InvLineIds.Contains(p.JobInvoiceLineId)
                                      select p).ToList();

                var ReceiveIds = InvoiceRecords.Select(m => m.JobReceiveLineId).ToArray();

                var ReceiveRecords = (from p in db.JobReceiveLine.AsNoTracking()
                                      where ReceiveIds.Contains(p.JobReceiveLineId)
                                      select p).ToList();

                var OrderLineIds = ReceiveRecords.Select(m => m.JobOrderLineId).ToArray();

                var OrderRecords = (from p in db.JobOrderLine.AsNoTracking()
                                    where OrderLineIds
                                    .Contains(p.JobOrderLineId)
                                    select p).ToList();


                foreach (var item in vm.JobInvoiceReturnLineViewModel)
                {
                    decimal balqty = InvBalRecords.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().BalQty;

                    var InvRecord = InvoiceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault();
                    var RecRecord = ReceiveRecords.Where(m => m.JobReceiveLineId == InvRecord.JobReceiveLineId).FirstOrDefault();

                    if (RecRecord.ProductUidId.HasValue && RecRecord.ProductUidId.Value > 0 && item.Qty > 0 && item.Qty <= balqty)
                        BarCodeBased.Add(item);

                    if (item.Qty > 0 && item.Qty <= balqty && !RecRecord.ProductUidId.HasValue)
                    {

                        JobInvoiceReturnLine line = new JobInvoiceReturnLine();

                        line.JobInvoiceReturnHeaderId = item.JobInvoiceReturnHeaderId;
                        line.JobInvoiceLineId = item.JobInvoiceLineId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        //line.DiscountPer = item.DiscountPer;
                        line.Rate = item.Rate;
                        line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Amount = item.Rate * line.DealQty;
                        line.SalesTaxGroupProductId = item.SalesTaxGroupProductId;
                        line.Remark = item.Remark;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.JobInvoiceReturnLineId = pk;

                        LineStatus.Add(line.JobInvoiceLineId, line.Qty);


                        JobReturnLine GLine = Mapper.Map<JobInvoiceReturnLine, JobReturnLine>(line);
                        GLine.JobReceiveLineId = InvRecord.JobReceiveLineId;
                        GLine.JobReturnHeaderId = GoodsRetHeader.JobReturnHeaderId;
                        GLine.JobReturnLineId = Gpk;
                        GLine.Qty = line.Qty;

                        JobReceiveLine JobReceiveLine = ReceiveRecords.Where(m => m.JobReceiveLineId == GLine.JobReceiveLineId).FirstOrDefault();
                        JobOrderLine JobOrderLine = OrderRecords.Where(m => m.JobOrderLineId == JobReceiveLine.JobOrderLineId).FirstOrDefault();

                        if (JobReceiveLine.Weight != 0)
                        {
                            decimal UnitWeight = JobReceiveLine.Weight / JobReceiveLine.Qty;
                            GLine.Weight = UnitWeight * line.Qty;
                        }

                        LineStatusWeights.Add(line.JobInvoiceLineId, GLine.Weight);




                        if (Settings.isPostedInStock ?? false)
                        {
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
                            StockViewModel.DocHeaderId = GoodsRetHeader.JobReturnHeaderId;
                            StockViewModel.DocLineId = GLine.JobReturnLineId;
                            StockViewModel.DocTypeId = GoodsRetHeader.DocTypeId;
                            StockViewModel.StockHeaderDocDate = GoodsRetHeader.DocDate;
                            StockViewModel.StockDocDate = GoodsRetHeader.DocDate;
                            StockViewModel.DocNo = GoodsRetHeader.DocNo;
                            StockViewModel.DivisionId = GoodsRetHeader.DivisionId;
                            StockViewModel.SiteId = GoodsRetHeader.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.PersonId = GoodsRetHeader.JobWorkerId;
                            StockViewModel.ProductId = item.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = GoodsRetHeader.GodownId;
                            StockViewModel.HeaderProcessId = GoodsRetHeader.ProcessId;
                            StockViewModel.GodownId = GoodsRetHeader.GodownId;
                            StockViewModel.Remark = GoodsRetHeader.Remark;
                            StockViewModel.Status = GoodsRetHeader.Status;
                            StockViewModel.ProcessId = GoodsRetHeader.ProcessId;
                            StockViewModel.LotNo = null;
                            StockViewModel.CostCenterId = null;
                            StockViewModel.Qty_Iss = GLine.Qty;
                            StockViewModel.Qty_Rec = 0;
                            StockViewModel.Rate = null;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = item.Specification;
                            StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockViewModel.CreatedBy = User.Identity.Name;
                            StockViewModel.ProductUidId = RecRecord.ProductUidId;
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
                                GoodsRetHeader.StockHeaderId = StockViewModel.StockHeaderId;
                            }


                            GLine.StockId = StockViewModel.StockId;
                        }





                        if (Settings.isPostedInStockProcess ?? false)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (GoodsRetHeader.StockHeaderId != null && GoodsRetHeader.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)GoodsRetHeader.StockHeaderId;
                            }
                            else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
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
                            StockProcessViewModel.DocHeaderId = GoodsRetHeader.JobReturnHeaderId;
                            StockProcessViewModel.DocLineId = GLine.JobReturnLineId;
                            StockProcessViewModel.DocTypeId = GoodsRetHeader.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = GoodsRetHeader.DocDate;
                            StockProcessViewModel.StockProcessDocDate = GoodsRetHeader.DocDate;
                            StockProcessViewModel.DocNo = GoodsRetHeader.DocNo;
                            StockProcessViewModel.DivisionId = GoodsRetHeader.DivisionId;
                            StockProcessViewModel.SiteId = GoodsRetHeader.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = GoodsRetHeader.JobWorkerId;
                            StockProcessViewModel.ProductId = item.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = GoodsRetHeader.GodownId;
                            StockProcessViewModel.HeaderProcessId = GoodsRetHeader.ProcessId;
                            StockProcessViewModel.GodownId = GoodsRetHeader.GodownId;
                            StockProcessViewModel.Remark = GoodsRetHeader.Remark;
                            StockProcessViewModel.Status = GoodsRetHeader.Status;
                            StockProcessViewModel.ProcessId = GoodsRetHeader.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = null;
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = GLine.Qty;
                            StockProcessViewModel.Rate = null;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = item.Specification;
                            StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = item.Dimension4Id;
                            StockProcessViewModel.ProductUidId = RecRecord.ProductUidId;
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
                                    GoodsRetHeader.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                }
                            }


                            GLine.StockProcessId = StockProcessViewModel.StockProcessId;
                        }


                        if (JobReceiveLine != null && GLine.StockId != null)
                        {
                            if (JobReceiveLine.StockId != null && JobReceiveLine.StockId != 0)
                            {
                                StockAdj Adj_IssQty = new StockAdj();
                                Adj_IssQty.StockInId = (int)JobReceiveLine.StockId;
                                Adj_IssQty.StockOutId = (int)GLine.StockId;
                                Adj_IssQty.DivisionId = Header.DivisionId;
                                Adj_IssQty.SiteId = Header.SiteId;
                                Adj_IssQty.AdjustedQty = GLine.Qty;
                                Adj_IssQty.ObjectState = Model.ObjectState.Added;
                                db.StockAdj.Add(Adj_IssQty);
                            }
                        }


                        GLine.ObjectState = Model.ObjectState.Added;
                        db.JobReturnLine.Add(GLine);

                        line.JobReturnLineId = GLine.JobReturnLineId;
                        line.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceReturnLine.Add(line);

                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobInvoiceReturnLineId, HeaderTableId = item.JobInvoiceReturnHeaderId, PersonID = Header.JobWorkerId, DealQty = line.DealQty });
                        RefIds.Add(new LineReferenceIds { LineId = line.JobInvoiceReturnLineId, RefLineId = line.JobInvoiceLineId });
                        Gpk++;
                        pk++;

                        List<CalculationProductViewModel> ChargeRates = new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId,
                            Header.ProcessId, item.SalesTaxGroupPersonId, item.SalesTaxGroupProductId).ToList();
                        if (ChargeRates != null)
                        {
                            LineChargeRates.Add(new LineChargeRates { LineId = line.JobInvoiceReturnLineId, ChargeRates = ChargeRates });
                        }

                        Cnt = Cnt + 1;
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {

                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;



                    //GoodsRetHeader.Status = (int)StatusConstants.Modified;
                    GoodsRetHeader.ModifiedBy = User.Identity.Name;
                    GoodsRetHeader.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobInvoiceReturnHeader.Add(Header);

                GoodsRetHeader.ObjectState = Model.ObjectState.Modified;
                db.JobReturnHeader.Add(GoodsRetHeader);

                int[] RecLineIds = null;
                RecLineIds = RefIds.Select(m => m.RefLineId).ToArray();

                var Charges = (from p in db.JobInvoiceLine
                               where RecLineIds.Contains(p.JobInvoiceLineId)
                               join LineCharge in db.JobInvoiceLineCharge on p.JobInvoiceLineId equals LineCharge.LineTableId
                               join HeaderCharge in db.JobInvoiceHeaderCharges on p.JobInvoiceHeaderId equals HeaderCharge.HeaderTableId
                               group new { p, LineCharge, HeaderCharge } by new { p.JobInvoiceLineId } into g
                               select new
                               {
                                   LineId = g.Key.JobInvoiceLineId,
                                   HeaderCharges = g.Select(m => m.HeaderCharge).ToList(),
                                   Linecharges = g.Select(m => m.LineCharge).ToList(),
                               }).ToList();



                var LineListWithReferences = (from p in LineList
                                              join t in RefIds on p.LineTableId equals t.LineId
                                              join t2 in Charges on t.RefLineId equals t2.LineId into table
                                              from LineLis in table.DefaultIfEmpty()
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
                                                  RLineCharges = (LineLis == null ? null : Mapper.Map<List<LineChargeViewModel>>(LineLis.Linecharges)),
                                                  ChargeRates = LineChargeRatesTab.ChargeRates,
                                              }).ToList();

                new JobInvoiceLineStatusService(db).UpdateJobInvoiceQtyReturnMultiple(LineStatus, Header.DocDate, ref db, LineStatusWeights);

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineListWithReferences, vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceReturnHeaderCharges", "Web.JobInvoiceReturnLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    JobInvoiceReturnLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobInvoiceReturnLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceReturnLineCharge.Add(PoLineCharge);
                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        JobInvoiceReturnHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobInvoiceReturnHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId;
                        POHeaderCharge.PersonID = Header.JobWorkerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceReturnHeaderCharge.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new JobInvoiceReturnHeaderChargeService(db).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceReturnHeaderCharge.Add(footercharge);
                    }

                }

                try
                {
                    JobInvoiceReturnDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId), ref db);
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

                    //_unitOfWork.Save();
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
                    JobInvoiceReturnDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceReturnLineViewModel.FirstOrDefault().JobInvoiceReturnHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                if (BarCodeBased.Count() > 0)
                {
                    List<JobInvoiceReturnBarCodeSequenceViewModel> Sequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
                    List<JobInvoiceReturnBarCodeSequenceViewModel> GSequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
                    foreach (var item in BarCodeBased)
                        Sequence.Add(new JobInvoiceReturnBarCodeSequenceViewModel
                        {
                            JobInvoiceReturnHeaderId = item.JobInvoiceReturnHeaderId,
                            JobInvoiceLineId = item.JobInvoiceLineId,
                            ProductName = item.ProductName,
                            Qty = item.Qty,
                            BalanceQty = item.InvoiceBalQty,
                        });


                    GSequence = (from p in Sequence
                                 orderby p.ProductName
                                 group p by p.ProductName into g
                                 select new JobInvoiceReturnBarCodeSequenceViewModel
                                 {
                                     JobInvoiceReturnHeaderId = g.Max(m => m.JobInvoiceReturnHeaderId),
                                     ProductName = g.Key,
                                     SJobInvLineIds = string.Join(",", g.Select(m => m.JobInvoiceLineId).ToList()),
                                     Qty = g.Sum(m => m.Qty),
                                     BalanceQty = g.Sum(m => m.BalanceQty),
                                 }).ToList();


                    JobInvoiceReturnBarCodeSequenceListViewModel SquenceList = new JobInvoiceReturnBarCodeSequenceListViewModel();
                    SquenceList.JobInvoiceReturnBarCodeSequenceViewModel = GSequence;
                    SquenceList.GodownId = GoodsRetHeader.GodownId;
                    return PartialView("_Sequence", SquenceList);

                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _SequencePost(JobInvoiceReturnBarCodeSequenceListViewModel vm)
        {
            List<JobInvoiceReturnBarCodeSequenceViewModel> Sequence = new List<JobInvoiceReturnBarCodeSequenceViewModel>();
            JobInvoiceReturnBarCodeSequenceListViewModel SquenceList = new JobInvoiceReturnBarCodeSequenceListViewModel();

            JobInvoiceReturnHeader Header = new JobInvoiceReturnHeaderService(db).Find(vm.JobInvoiceReturnBarCodeSequenceViewModel.FirstOrDefault().JobInvoiceReturnHeaderId);
            JobReturnHeader RetHeader = db.JobReturnHeader.Find(Header.JobReturnHeaderId);

            foreach (var item in vm.JobInvoiceReturnBarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                JobInvoiceReturnBarCodeSequenceViewModel SequenceLine = new JobInvoiceReturnBarCodeSequenceViewModel();

                SequenceLine.JobInvoiceReturnHeaderId = item.JobInvoiceReturnHeaderId;
                SequenceLine.JobInvoiceLineId = item.JobInvoiceLineId;
                SequenceLine.ProductName = item.ProductName;
                SequenceLine.SJobInvLineIds = item.SJobInvLineIds;


                if (!string.IsNullOrEmpty(item.ProductUidIdName))
                {
                    var BarCodes = _JobInvoiceReturnLineService.GetPendingBarCodesList(SequenceLine.SJobInvLineIds, RetHeader.GodownId);

                    var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SequenceLine.ProductUidIds = Ids;
                }
                else
                {
                    var BarCodes = _JobInvoiceReturnLineService.GetPendingBarCodesList(SequenceLine.SJobInvLineIds, RetHeader.GodownId);

                    var temp = BarCodes.Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SequenceLine.ProductUidIds = Ids;
                }
                SequenceLine.Qty = SequenceLine.ProductUidIds.Split(',').Count();
                Sequence.Add(SequenceLine);

            }
            SquenceList.JobInvoiceReturnBarCodeSequenceViewModelPost = Sequence;
            return PartialView("_BarCodes", SquenceList);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _BarCodesPost(JobInvoiceReturnBarCodeSequenceListViewModel vm)
        {
            int Cnt = 0;
            int Gpk = 0;
            int Pk = 0;
            int Serial = _JobInvoiceReturnLineService.GetMaxSr(vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId);
            bool HeaderChargeEdit = false;

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            Dictionary<int, decimal> LineStatusWeights = new Dictionary<int, decimal>();

            //s[] JobReceiveLinesIds = vm.JobInvoiceReturnBarCodeSequenceViewModel.Select(m => m.SJobRecLineIds).ToList().ToArray();

            string Temp = string.Join(",", vm.JobInvoiceReturnBarCodeSequenceViewModelPost.Select(m => m.SJobInvLineIds).ToList());

            int[] Ids = Temp.Split(',').Select(Int32.Parse).ToArray();

            var JobInvoiceRecords = (from p in db.JobInvoiceLine
                                     where Ids.Contains(p.JobInvoiceLineId)
                                     select p).ToList();

            var JobRecIds = JobInvoiceRecords.Select(m => m.JobReceiveLineId).ToArray();

            var JobReceiveRecords = (from p in db.JobReceiveLine
                                     where JobRecIds.Contains(p.JobReceiveLineId)
                                     select p).ToList();

            var JobInvoiceBalanceQtys = (from p in db.ViewJobInvoiceBalance
                                         where Ids.Contains(p.JobInvoiceLineId)
                                         select new
                                         {
                                             p.BalanceQty,
                                             p.JobInvoiceLineId,
                                         }).ToList();


            var JobOrders = (from p in db.JobReceiveLine
                             join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                             where JobRecIds.Contains(p.JobReceiveLineId)
                             select t).ToList();



            var BarCode = string.Join(",", vm.JobInvoiceReturnBarCodeSequenceViewModelPost.Select(m => m.ProductUidIds).ToList());
            int[] BarCodeIds = BarCode.Split(',').Select(Int32.Parse).ToArray();

            var BarCodeRecords = (from p in db.ProductUid
                                  where BarCodeIds.Contains(p.ProductUIDId)
                                  select p).ToList();

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReturnDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobInvoiceReturnHeader Header = new JobInvoiceReturnHeaderService(db).Find(vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId);
                JobReturnHeader RetHeader = new JobReturnHeaderService(_unitOfWork).Find(Header.JobReturnHeaderId.Value);
                var Site = new SiteService(_unitOfWork).FindByPerson(Header.JobWorkerId);
                JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                int? MaxLineId = new JobInvoiceReturnLineChargeService(db).GetMaxProductCharge(Header.JobInvoiceReturnHeaderId, "Web.JobInvoiceReturnLines", "JobInvoiceReturnHeaderId", "JobInvoiceReturnLineId");
                int CalculationId = Settings.CalculationId.Value;
                int PersonCount = 0;

                List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

                foreach (var item in vm.JobInvoiceReturnBarCodeSequenceViewModelPost)
                {
                    //decimal balqty = (from p in db.ViewJobReceiveBalance
                    //                  where p.JobReceiveLineId== item.JobReceiveLineId
                    //                  select p.BalanceQty).FirstOrDefault();

                    decimal balqty = (from p in JobInvoiceBalanceQtys
                                      where item.SJobInvLineIds.Contains(p.JobInvoiceLineId.ToString())
                                      select p.BalanceQty).ToList().Sum();




                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {
                            JobReceiveLine Receive = JobReceiveRecords.Where(m => m.ProductUidId == BarCodes).FirstOrDefault();
                            JobInvoiceLine Invoice = JobInvoiceRecords.Where(m => m.JobReceiveLineId == Receive.JobReceiveLineId).FirstOrDefault();
                            JobOrderLine Order = JobOrders.Where(m => m.JobOrderLineId == Receive.JobOrderLineId).FirstOrDefault();

                            JobInvoiceReturnLine Line = new JobInvoiceReturnLine();
                            Line.JobInvoiceReturnHeaderId = item.JobInvoiceReturnHeaderId;
                            Line.JobInvoiceLineId = Invoice.JobInvoiceLineId;
                            Line.Qty = 1;
                            Line.Sr = Serial++;
                            Line.Rate = Invoice.Rate;
                            Line.Amount = Invoice.Amount;
                            Line.DealQty = Invoice.DealQty;
                            Line.DealUnitId = Invoice.DealUnitId;
                            Line.UnitConversionMultiplier = Invoice.UnitConversionMultiplier;
                            Line.CreatedDate = DateTime.Now;
                            Line.ModifiedDate = DateTime.Now;
                            Line.CreatedBy = User.Identity.Name;
                            Line.ModifiedBy = User.Identity.Name;
                            Line.JobInvoiceReturnLineId = Pk++;

                            LineStatus.Add(Line.JobInvoiceLineId, Line.Qty);

                            JobReturnLine GLine = Mapper.Map<JobInvoiceReturnLine, JobReturnLine>(Line);
                            GLine.JobReceiveLineId = Invoice.JobReceiveLineId;
                            GLine.JobReturnHeaderId = RetHeader.JobReturnHeaderId;
                            GLine.JobReturnLineId = Gpk++;
                            GLine.Qty = Line.Qty;


                            if (Receive.Weight != 0)
                            {
                                decimal UnitWeight = Receive.Weight / Receive.Qty;
                                GLine.Weight = UnitWeight * Line.Qty;
                            }
                            LineStatusWeights.Add(Line.JobInvoiceLineId, GLine.Weight);

                            ProductUid Uid = BarCodeRecords.Where(m => m.ProductUIDId == BarCodes).FirstOrDefault();

                            GLine.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            GLine.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            GLine.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            GLine.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            GLine.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            GLine.ProductUidStatus = Uid.Status;
                            GLine.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            GLine.ProductUidCurrentGodownId = Uid.CurrenctGodownId;

                            if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                            {

                                Uid.LastTransactionDocId = RetHeader.JobReturnHeaderId;
                                Uid.LastTransactionDocDate = RetHeader.DocDate;
                                Uid.LastTransactionDocNo = RetHeader.DocNo;
                                Uid.LastTransactionDocTypeId = RetHeader.DocTypeId;
                                Uid.LastTransactionLineId = Line.JobInvoiceReturnLineId;
                                Uid.LastTransactionPersonId = RetHeader.JobWorkerId;
                                Uid.Status = ProductUidStatusConstants.Return;
                                Uid.CurrenctProcessId = RetHeader.ProcessId;

                                if (Site != null)
                                    Uid.CurrenctGodownId = Site.DefaultGodownId;
                                else
                                    Uid.CurrenctGodownId = null;

                                Uid.ModifiedBy = User.Identity.Name;
                                Uid.ModifiedDate = DateTime.Now;
                                //new ProductUidService(_unitOfWork).Update(Uid);
                                Uid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(Uid);

                            }


                            if (Settings.isPostedInStock ?? false)
                            {
                                StockViewModel StockViewModel = new StockViewModel();

                                if (Cnt == 0)
                                {
                                    StockViewModel.StockHeaderId = RetHeader.StockHeaderId ?? 0;
                                }
                                else
                                {
                                    if (RetHeader.StockHeaderId != null && RetHeader.StockHeaderId != 0)
                                    {
                                        StockViewModel.StockHeaderId = (int)RetHeader.StockHeaderId;
                                    }
                                    else
                                    {
                                        StockViewModel.StockHeaderId = -1;
                                    }

                                }

                                StockViewModel.StockId = -Cnt;
                                StockViewModel.DocHeaderId = RetHeader.JobReturnHeaderId;
                                StockViewModel.DocLineId = GLine.JobReturnLineId;
                                StockViewModel.DocTypeId = RetHeader.DocTypeId;
                                StockViewModel.StockHeaderDocDate = RetHeader.DocDate;
                                StockViewModel.StockDocDate = RetHeader.DocDate;
                                StockViewModel.DocNo = RetHeader.DocNo;
                                StockViewModel.DivisionId = RetHeader.DivisionId;
                                StockViewModel.SiteId = RetHeader.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.PersonId = RetHeader.JobWorkerId;
                                StockViewModel.ProductId = Order.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = RetHeader.GodownId;
                                StockViewModel.HeaderProcessId = RetHeader.ProcessId;
                                StockViewModel.GodownId = RetHeader.GodownId;
                                StockViewModel.Remark = RetHeader.Remark;
                                StockViewModel.Status = RetHeader.Status;
                                StockViewModel.ProcessId = RetHeader.ProcessId;
                                StockViewModel.LotNo = null;
                                StockViewModel.CostCenterId = null;
                                StockViewModel.Qty_Iss = GLine.Qty;
                                StockViewModel.Qty_Rec = 0;
                                StockViewModel.Rate = null;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = Order.Specification;
                                StockViewModel.Dimension1Id = Order.Dimension1Id;
                                StockViewModel.Dimension2Id = Order.Dimension2Id;
                                StockViewModel.Dimension3Id = Order.Dimension3Id;
                                StockViewModel.Dimension4Id = Order.Dimension4Id;
                                StockViewModel.ProductUidId = Receive.ProductUidId;
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
                                    RetHeader.StockHeaderId = StockViewModel.StockHeaderId;
                                }


                                GLine.StockId = StockViewModel.StockId;
                            }





                            if (Settings.isPostedInStockProcess ?? false)
                            {
                                StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                                if (RetHeader.StockHeaderId != null && RetHeader.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                                {
                                    StockProcessViewModel.StockHeaderId = (int)RetHeader.StockHeaderId;
                                }
                                else if (Settings.isPostedInStock ?? false)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
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
                                StockProcessViewModel.DocHeaderId = RetHeader.JobReturnHeaderId;
                                StockProcessViewModel.DocLineId = GLine.JobReturnLineId;
                                StockProcessViewModel.DocTypeId = RetHeader.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = RetHeader.DocDate;
                                StockProcessViewModel.StockProcessDocDate = RetHeader.DocDate;
                                StockProcessViewModel.DocNo = RetHeader.DocNo;
                                StockProcessViewModel.DivisionId = RetHeader.DivisionId;
                                StockProcessViewModel.SiteId = RetHeader.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.PersonId = RetHeader.JobWorkerId;
                                StockProcessViewModel.ProductId = Order.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                StockProcessViewModel.HeaderGodownId = RetHeader.GodownId;
                                StockProcessViewModel.HeaderProcessId = RetHeader.ProcessId;
                                StockProcessViewModel.GodownId = RetHeader.GodownId;
                                StockProcessViewModel.Remark = RetHeader.Remark;
                                StockProcessViewModel.Status = RetHeader.Status;
                                StockProcessViewModel.ProcessId = RetHeader.ProcessId;
                                StockProcessViewModel.LotNo = null;
                                StockProcessViewModel.CostCenterId = null;
                                StockProcessViewModel.Qty_Iss = 0;
                                StockProcessViewModel.Qty_Rec = GLine.Qty;
                                StockProcessViewModel.Rate = null;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = Order.Specification;
                                StockProcessViewModel.Dimension1Id = Order.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = Order.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = Order.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = Order.Dimension4Id;
                                StockProcessViewModel.ProductUidId = Receive.ProductUidId;
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
                                        RetHeader.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                    }
                                }


                                GLine.StockProcessId = StockProcessViewModel.StockProcessId;
                            }

                            GLine.ObjectState = Model.ObjectState.Added;
                            db.JobReturnLine.Add(GLine);

                            Line.JobReturnLineId = GLine.JobReturnLineId;
                            Line.ObjectState = Model.ObjectState.Added;
                            db.JobInvoiceReturnLine.Add(Line);
                                                      

                            Cnt = Cnt + 1;

                            LineList.Add(new LineDetailListViewModel { Amount = Line.Amount, Rate = Line.Rate, LineTableId = Line.JobInvoiceReturnLineId, HeaderTableId = item.JobInvoiceReturnHeaderId, PersonID = Header.JobWorkerId, DealQty = Line.DealQty });

                        }
                    }

                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;

                    //RetHeader.Status = (int)StatusConstants.Modified;
                    RetHeader.ModifiedBy = User.Identity.Name;
                    RetHeader.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobInvoiceReturnHeader.Add(Header);

                RetHeader.ObjectState = Model.ObjectState.Modified;
                db.JobReturnHeader.Add(RetHeader);

                new JobInvoiceLineStatusService(db).UpdateJobInvoiceQtyReturnMultiple(LineStatus, Header.DocDate, ref db, LineStatusWeights);
                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceReturnHeaderCharges", "Web.JobInvoiceReturnLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    JobInvoiceReturnLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobInvoiceReturnLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceReturnLineCharge.Add(PoLineCharge);
                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        JobInvoiceReturnHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobInvoiceReturnHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId;
                        POHeaderCharge.PersonID = Header.JobWorkerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceReturnHeaderCharge.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new JobInvoiceReturnHeaderChargeService(db).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceReturnHeaderCharge.Add(footercharge);
                    }

                }


                try
                {
                    JobInvoiceReturnDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId), ref db);
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
                    return PartialView("_Results", vm);
                }

                try
                {
                    JobInvoiceReturnDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobInvoiceReturnHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(JobInvoiceReturnLineViewModel vm)
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
            JobInvoiceReturnHeader H = new JobInvoiceReturnHeaderService(db).Find(Id);
            JobInvoiceReturnLineViewModel s = new JobInvoiceReturnLineViewModel();

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.Nature = H.Nature;
            s.JobInvoiceReturnHeaderId = H.JobInvoiceReturnHeaderId;
            s.JobInvoiceReturnHeaderDocNo = H.DocNo;
            s.JobWorkerId = sid;
            s.DocTypeId = H.DocTypeId;
            s.DivisionId = H.DivisionId;


            if (H.SalesTaxGroupPersonId != null)
                s.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (s.CalculationId == null)
                s.CalculationId = settings.CalculationId;

            s.SiteId = H.SiteId;
            ViewBag.LineMode = "Create";
            //PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobInvoiceReturnLineViewModel svm)
        {
            JobInvoiceReturnHeader temp = new JobInvoiceReturnHeaderService(db).Find(svm.JobInvoiceReturnHeaderId);

            if (svm.JobInvoiceReturnLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            bool BeforeSave = true;

            try
            {
                if (svm.JobInvoiceReturnLineId <= 0)
                    BeforeSave = JobInvoiceReturnDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceReturnDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.JobInvoiceReturnLineId <= 0)
            {


                decimal balqty = (from p in db.JobInvoiceLine
                                  where p.JobInvoiceLineId == svm.JobInvoiceLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }

                if (svm.Nature != TransactionNatureConstants.Debit && svm.Nature != TransactionNatureConstants.Credit)
                {
                    if (svm.Qty == 0)
                    {
                        ModelState.AddModelError("Qty", "Please Check Qty");
                    }
                }

                if (svm.JobInvoiceLineId <= 0)
                {
                    ModelState.AddModelError("JobInvoiceLineId", "Job Invoice field is required");
                }

                var tenp = ModelState.Where(m => m.Value.Errors.Count() > 0);

                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    JobInvoiceReturnLine s = Mapper.Map<JobInvoiceReturnLineViewModel, JobInvoiceReturnLine>(svm);
                    s.Sr = _JobInvoiceReturnLineService.GetMaxSr(s.JobInvoiceReturnHeaderId);
                    //s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;


                    if (temp.JobReturnHeaderId.HasValue)
                    {
                        JobReturnHeader JobReturnHeader = new JobReturnHeaderService(_unitOfWork).Find((int)temp.JobReturnHeaderId);

                        JobReturnLine Gline = Mapper.Map<JobInvoiceReturnLine, JobReturnLine>(s);
                        Gline.JobReceiveLineId = new JobInvoiceLineService(_unitOfWork).Find(s.JobInvoiceLineId).JobReceiveLineId;
                        Gline.JobReturnHeaderId = temp.JobReturnHeaderId ?? 0;
                        Gline.Qty = svm.Qty;
                        Gline.Weight = svm.Weight;

                        new JobInvoiceLineStatusService(db).UpdateJobInvoiceQtyOnReturn(s.JobInvoiceLineId, s.JobInvoiceReturnLineId, JobReturnHeader.DocDate, s.Qty, Gline.Weight, ref db);


                        if (svm.JobInvoiceSettings.isPostedInStock)
                        {
                            StockViewModel StockViewModel = new StockViewModel();
                            StockViewModel.StockHeaderId = JobReturnHeader.StockHeaderId ?? 0;
                            StockViewModel.DocHeaderId = JobReturnHeader.JobReturnHeaderId;
                            StockViewModel.DocLineId = Gline.JobReturnLineId;
                            StockViewModel.DocTypeId = JobReturnHeader.DocTypeId;
                            StockViewModel.StockHeaderDocDate = JobReturnHeader.DocDate;
                            StockViewModel.StockDocDate = JobReturnHeader.DocDate;
                            StockViewModel.DocNo = JobReturnHeader.DocNo;
                            StockViewModel.DivisionId = JobReturnHeader.DivisionId;
                            StockViewModel.SiteId = JobReturnHeader.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.HeaderProcessId = JobReturnHeader.ProcessId;
                            StockViewModel.PersonId = JobReturnHeader.JobWorkerId;
                            StockViewModel.ProductId = svm.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = JobReturnHeader.GodownId;
                            StockViewModel.GodownId = JobReturnHeader.GodownId;
                            StockViewModel.ProcessId = JobReturnHeader.ProcessId;
                            StockViewModel.LotNo = null;
                            StockViewModel.CostCenterId = null;
                            StockViewModel.Qty_Iss = Gline.Qty;
                            StockViewModel.Qty_Rec = 0;
                            StockViewModel.Rate = null;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = svm.Specification;
                            StockViewModel.Dimension1Id = svm.Dimension1Id;
                            StockViewModel.Dimension2Id = svm.Dimension2Id;
                            StockViewModel.Dimension3Id = svm.Dimension3Id;
                            StockViewModel.Dimension4Id = svm.Dimension4Id;
                            StockViewModel.Remark = Gline.Remark;
                            StockViewModel.ProductUidId = svm.ProductUidId;
                            StockViewModel.Status = JobReturnHeader.Status;
                            StockViewModel.CreatedBy = JobReturnHeader.CreatedBy;
                            StockViewModel.CreatedDate = DateTime.Now;
                            StockViewModel.ModifiedBy = JobReturnHeader.ModifiedBy;
                            StockViewModel.ModifiedDate = DateTime.Now;

                            string StockPostingError = "";
                            StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                            if (StockPostingError != "")
                            {
                                ModelState.AddModelError("", StockPostingError);
                                return PartialView("_Create", svm);
                            }

                            Gline.StockId = StockViewModel.StockId;

                            if (JobReturnHeader.StockHeaderId == null)
                            {
                                JobReturnHeader.StockHeaderId = StockViewModel.StockHeaderId;
                            }
                        }



                        if (svm.JobInvoiceSettings.isPostedInStockProcess)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (JobReturnHeader.StockHeaderId != null && JobReturnHeader.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)JobReturnHeader.StockHeaderId;
                            }
                            else if (svm.JobInvoiceSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessViewModel.StockHeaderId = 0;
                            }


                            StockProcessViewModel.DocHeaderId = JobReturnHeader.JobReturnHeaderId;
                            StockProcessViewModel.DocLineId = Gline.JobReturnLineId;
                            StockProcessViewModel.DocTypeId = JobReturnHeader.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = JobReturnHeader.DocDate;
                            StockProcessViewModel.StockProcessDocDate = JobReturnHeader.DocDate;
                            StockProcessViewModel.DocNo = JobReturnHeader.DocNo;
                            StockProcessViewModel.DivisionId = JobReturnHeader.DivisionId;
                            StockProcessViewModel.SiteId = JobReturnHeader.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.HeaderProcessId = JobReturnHeader.ProcessId;
                            StockProcessViewModel.PersonId = JobReturnHeader.JobWorkerId;
                            StockProcessViewModel.ProductId = svm.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = JobReturnHeader.GodownId;
                            StockProcessViewModel.GodownId = JobReturnHeader.GodownId;
                            StockProcessViewModel.ProcessId = JobReturnHeader.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = null;
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = Gline.Qty;
                            StockProcessViewModel.Rate = null;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = svm.Specification;
                            StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                            StockProcessViewModel.Remark = Gline.Remark;
                            StockProcessViewModel.ProductUidId = svm.ProductUidId;
                            StockProcessViewModel.Status = JobReturnHeader.Status;
                            StockProcessViewModel.CreatedBy = JobReturnHeader.CreatedBy;
                            StockProcessViewModel.CreatedDate = DateTime.Now;
                            StockProcessViewModel.ModifiedBy = JobReturnHeader.ModifiedBy;
                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                ModelState.AddModelError("", StockProcessPostingError);
                                return PartialView("_Create", svm);
                            }

                            Gline.StockProcessId = StockProcessViewModel.StockProcessId;

                            if (svm.JobInvoiceSettings.isPostedInStock == false)
                            {
                                if (JobReturnHeader.StockHeaderId == null)
                                {
                                    JobReturnHeader.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                }
                            }
                        }



                        var JobReceiveLine = db.JobReceiveLine.Find(Gline.JobReceiveLineId);

                        if (JobReceiveLine != null)
                        {
                            if (JobReceiveLine.StockId != null && JobReceiveLine.StockId != 0 && Gline.StockId != null)
                            {
                                StockAdj Adj_IssQty = new StockAdj();
                                Adj_IssQty.StockInId = (int)JobReceiveLine.StockId;
                                Adj_IssQty.StockOutId = (int)Gline.StockId;
                                Adj_IssQty.DivisionId = temp.DivisionId;
                                Adj_IssQty.SiteId = temp.SiteId;
                                Adj_IssQty.AdjustedQty = s.Qty;
                                Adj_IssQty.ObjectState = Model.ObjectState.Added;
                                db.StockAdj.Add(Adj_IssQty);
                            }
                        }


                        if (JobReceiveLine.ProductUidId.HasValue && JobReceiveLine.ProductUidId > 0)
                        {
                            ProductUid Uid = new ProductUidService(_unitOfWork).Find(JobReceiveLine.ProductUidId.Value);



                            Gline.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            Gline.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            Gline.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            Gline.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            Gline.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            Gline.ProductUidStatus = Uid.Status;
                            Gline.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            Gline.ProductUidCurrentGodownId = Uid.CurrenctGodownId;



                            if (JobReturnHeader.JobWorkerId == Uid.LastTransactionPersonId || JobReturnHeader.SiteId == 17)
                            {

                                Uid.LastTransactionDocId = JobReturnHeader.JobReturnHeaderId;
                                Uid.LastTransactionDocDate = JobReturnHeader.DocDate;
                                Uid.LastTransactionDocNo = JobReturnHeader.DocNo;
                                Uid.LastTransactionDocTypeId = JobReturnHeader.DocTypeId;
                                Uid.LastTransactionLineId = Gline.JobReturnLineId;
                                Uid.LastTransactionPersonId = JobReturnHeader.JobWorkerId;
                                Uid.Status = ProductUidStatusConstants.Return;
                                Uid.CurrenctProcessId = JobReturnHeader.ProcessId;

                                var Site = new SiteService(_unitOfWork).FindByPerson(JobReturnHeader.JobWorkerId);
                                if (Site != null)
                                    Uid.CurrenctGodownId = Site.DefaultGodownId;
                                else
                                    Uid.CurrenctGodownId = null;

                                Uid.ModifiedBy = User.Identity.Name;
                                Uid.ModifiedDate = DateTime.Now;
                                Uid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(Uid);

                            }

                        }


                        Gline.ObjectState = Model.ObjectState.Added;
                        db.JobReturnLine.Add(Gline);
                        s.JobReturnLineId = Gline.JobReturnLineId;



                        if (JobReturnHeader.Status != (int)StatusConstants.Drafted && JobReturnHeader.Status != (int)StatusConstants.Import)
                        {
                            JobReturnHeader.Status = (int)StatusConstants.Modified;
                            JobReturnHeader.ModifiedBy = User.Identity.Name;
                            JobReturnHeader.ModifiedDate = DateTime.Now;
                            JobReturnHeader.ObjectState = Model.ObjectState.Modified;
                            db.JobReturnHeader.Add(JobReturnHeader);
                        }     
                    }

                    s.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceReturnLine.Add(s);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.JobInvoiceReturnLineId;
                            item.PersonID = temp.JobWorkerId;
                            item.HeaderTableId = s.JobInvoiceReturnHeaderId;
                            item.CostCenterId = s.CostCenterId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.JobInvoiceReturnLineCharge.Add(item);

                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            if (item.Id > 0)
                            {

                                var footercharge = new JobInvoiceReturnHeaderChargeService(db).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.JobInvoiceReturnHeaderCharge.Add(footercharge);
                            }
                            else
                            {
                                item.HeaderTableId = s.JobInvoiceReturnHeaderId;
                                item.PersonID = temp.JobWorkerId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.JobInvoiceReturnHeaderCharge.Add(item);
                            }
                        }


                    JobInvoiceReturnHeader temp2 = new JobInvoiceReturnHeaderService(db).Find(s.JobInvoiceReturnHeaderId);
                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                    }                  

                    temp2.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceReturnHeader.Add(temp2);



                    try
                    {
                        JobInvoiceReturnDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobInvoiceReturnHeaderId, s.JobInvoiceReturnLineId, EventModeConstants.Add), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobInvoiceReturnDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobInvoiceReturnHeaderId, s.JobInvoiceReturnLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }
                    return RedirectToAction("_Create", new { id = s.JobInvoiceReturnHeaderId, sid = svm.JobWorkerId });
                }
                else
                {
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
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                JobInvoiceReturnLine line = _JobInvoiceReturnLineService.Find(svm.JobInvoiceReturnLineId);
                

                JobInvoiceReturnLine ExRec = new JobInvoiceReturnLine();
                ExRec = Mapper.Map<JobInvoiceReturnLine>(line);


                decimal balqty = (from p in db.JobInvoiceLine
                                  where p.JobInvoiceLineId == svm.JobInvoiceLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty + line.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }


                if (ModelState.IsValid)
                {
                    //line.DiscountPer = svm.DiscountPer;
                    line.SalesTaxGroupProductId = svm.SalesTaxGroupProductId;
                    line.CostCenterId = svm.CostCenterId;
                    line.Remark = svm.Remark;
                    line.Rate = svm.Rate;
                    line.Qty = svm.Qty;
                    line.DealQty = svm.DealQty;
                    line.Amount = svm.Amount;
                    line.ModifiedBy = User.Identity.Name;
                    line.ModifiedDate = DateTime.Now;

                    line.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceReturnLine.Add(line);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = line,
                    });


                    if (temp.JobReturnHeaderId.HasValue)
                    {
                        JobReturnHeader JobReturnHeader = new JobReturnHeaderService(_unitOfWork).Find((int)temp.JobReturnHeaderId);
                        JobReturnLine GLine = new JobReturnLineService(_unitOfWork).Find(line.JobReturnLineId ?? 0);

                        JobReturnLine ExRecR = new JobReturnLine();
                        ExRecR = Mapper.Map<JobReturnLine>(GLine);

                        GLine.Remark = line.Remark;
                        GLine.Qty = line.Qty;
                        GLine.DealQty = line.DealQty;
                        GLine.Weight = svm.Weight;

                        GLine.ObjectState = Model.ObjectState.Modified;
                        //new JobReturnLineService(_unitOfWork).Update(GLine);
                        db.JobReturnLine.Add(GLine);

                        new JobInvoiceLineStatusService(db).UpdateJobInvoiceQtyOnReturn(line.JobInvoiceLineId, line.JobInvoiceReturnLineId, temp.DocDate, line.Qty, GLine.Weight, ref db);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecR,
                            Obj = GLine,
                        });


                        if (GLine.StockId != null)
                        {
                            StockViewModel StockViewModel = new StockViewModel();
                            StockViewModel.StockHeaderId = JobReturnHeader.StockHeaderId ?? 0;
                            StockViewModel.StockId = GLine.StockId ?? 0;
                            StockViewModel.DocHeaderId = JobReturnHeader.JobReturnHeaderId;
                            StockViewModel.DocLineId = GLine.JobReceiveLineId;
                            StockViewModel.DocTypeId = JobReturnHeader.DocTypeId;
                            StockViewModel.StockHeaderDocDate = JobReturnHeader.DocDate;
                            StockViewModel.StockDocDate = JobReturnHeader.DocDate;
                            StockViewModel.DocNo = JobReturnHeader.DocNo;
                            StockViewModel.DivisionId = JobReturnHeader.DivisionId;
                            StockViewModel.SiteId = JobReturnHeader.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.HeaderProcessId = JobReturnHeader.ProcessId;
                            StockViewModel.PersonId = JobReturnHeader.JobWorkerId;
                            StockViewModel.ProductId = svm.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = JobReturnHeader.GodownId;
                            StockViewModel.GodownId = JobReturnHeader.GodownId;
                            StockViewModel.ProcessId = JobReturnHeader.ProcessId;
                            StockViewModel.LotNo = null;
                            StockViewModel.CostCenterId = null;
                            StockViewModel.Qty_Iss = svm.Qty;
                            StockViewModel.Qty_Rec = 0;
                            StockViewModel.Rate = null;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = svm.Specification;
                            StockViewModel.Dimension1Id = svm.Dimension1Id;
                            StockViewModel.Dimension2Id = svm.Dimension2Id;
                            StockViewModel.Dimension3Id = svm.Dimension3Id;
                            StockViewModel.Dimension4Id = svm.Dimension4Id;
                            StockViewModel.Remark = GLine.Remark;
                            StockViewModel.ProductUidId = svm.ProductUidId;
                            StockViewModel.Status = JobReturnHeader.Status;
                            StockViewModel.CreatedBy = JobReturnHeader.CreatedBy;
                            StockViewModel.CreatedDate = JobReturnHeader.CreatedDate;
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


                        if (GLine.StockProcessId != null)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                            StockProcessViewModel.StockHeaderId = JobReturnHeader.StockHeaderId ?? 0;
                            StockProcessViewModel.StockProcessId = GLine.StockProcessId ?? 0;
                            StockProcessViewModel.DocHeaderId = JobReturnHeader.JobReturnHeaderId;
                            StockProcessViewModel.DocLineId = GLine.JobReceiveLineId;
                            StockProcessViewModel.DocTypeId = JobReturnHeader.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = JobReturnHeader.DocDate;
                            StockProcessViewModel.StockProcessDocDate = JobReturnHeader.DocDate;
                            StockProcessViewModel.DocNo = JobReturnHeader.DocNo;
                            StockProcessViewModel.DivisionId = JobReturnHeader.DivisionId;
                            StockProcessViewModel.SiteId = JobReturnHeader.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.HeaderProcessId = JobReturnHeader.ProcessId;
                            StockProcessViewModel.PersonId = JobReturnHeader.JobWorkerId;
                            StockProcessViewModel.ProductId = svm.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = JobReturnHeader.GodownId;
                            StockProcessViewModel.GodownId = JobReturnHeader.GodownId;
                            StockProcessViewModel.ProcessId = JobReturnHeader.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = null;
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = svm.Qty;
                            StockProcessViewModel.Rate = null;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = svm.Specification;
                            StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                            StockProcessViewModel.Remark = GLine.Remark;
                            StockProcessViewModel.ProductUidId = svm.ProductUidId;
                            StockProcessViewModel.Status = JobReturnHeader.Status;
                            StockProcessViewModel.CreatedBy = JobReturnHeader.CreatedBy;
                            StockProcessViewModel.CreatedDate = JobReturnHeader.CreatedDate;
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

                        JobReturnHeader.ModifiedBy = User.Identity.Name;
                        JobReturnHeader.ModifiedDate = DateTime.Now;
                        JobReturnHeader.ObjectState = Model.ObjectState.Modified;
                        db.JobReturnHeader.Add(JobReturnHeader);


                        StockAdj Adj = (from L in db.StockAdj
                                        where L.StockOutId == GLine.StockId
                                        select L).FirstOrDefault();

                        if (Adj != null)
                        {
                            Adj.ObjectState = Model.ObjectState.Deleted;
                            db.StockAdj.Remove(Adj);
                            //new StockAdjService(_unitOfWork).Delete(Adj);
                        }

                        var JobReceiveLine = db.JobReceiveLine.Find(GLine.JobReceiveLineId);

                        if (JobReceiveLine != null)
                        {
                            if (JobReceiveLine.StockId != null && JobReceiveLine.StockId != 0)
                            {
                                StockAdj Adj_IssQty = new StockAdj();
                                Adj_IssQty.StockInId = (int)JobReceiveLine.StockId;
                                Adj_IssQty.StockOutId = (int)GLine.StockId;
                                Adj_IssQty.DivisionId = temp.DivisionId;
                                Adj_IssQty.SiteId = temp.SiteId;
                                Adj_IssQty.AdjustedQty = svm.Qty;
                                Adj_IssQty.ObjectState = Model.ObjectState.Added;
                                db.StockAdj.Add(Adj_IssQty);
                                //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                            }
                        }
                    }

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        //JobReturnHeader.Status = temp.Status;
                        //new JobInvoiceReturnHeaderService(_unitOfWork).Update(temp);
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceReturnHeader.Add(temp);






                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new JobInvoiceReturnLineChargeService(db).Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceReturnLineCharge.Add(productcharge);
                            //new JobInvoiceReturnLineChargeService(_unitOfWork).Update(productcharge);

                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new JobInvoiceReturnHeaderChargeService(db).Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;

                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceReturnHeaderCharge.Add(footercharge);
                            //new JobInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);
                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceReturnDocEvents.onLineSaveEvent(this, new JobEventArgs(line.JobInvoiceReturnHeaderId, line.JobInvoiceReturnLineId, EventModeConstants.Edit), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobInvoiceReturnDocEvents.afterLineSaveEvent(this, new JobEventArgs(line.JobInvoiceReturnHeaderId, line.JobInvoiceReturnLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    //Saving the Activity Log


                    //LogActivity.LogActivityDetail(temp.DocTypeId,
                    //temp.JobInvoiceReturnHeaderId,
                    //null,
                    //(int)ActivityTypeContants.Modified,
                    //"",
                    //User.Identity.Name, temp.DocNo, Modifications);

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


        private ActionResult _Modify(int id)
        {
            JobInvoiceReturnLineViewModel temp = _JobInvoiceReturnLineService.GetJobInvoiceReturnLine(id);

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


            JobInvoiceReturnHeader H = new JobInvoiceReturnHeaderService(db).Find(temp.JobInvoiceReturnHeaderId);

            temp.DocTypeId = H.DocTypeId;
            temp.SiteId = H.SiteId;
            temp.DivisionId = H.DivisionId;
            temp.SalesTaxGroupPersonId = H.SalesTaxGroupPersonId;

            PrepareViewBag(temp);
            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (H.SalesTaxGroupPersonId != null)
                temp.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (temp.CalculationId == null)
                temp.CalculationId = settings.CalculationId;



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

        private ActionResult _Delete(int id)
        {
            JobInvoiceReturnLineViewModel temp = _JobInvoiceReturnLineService.GetJobInvoiceReturnLine(id);

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

            PrepareViewBag(temp);

            JobInvoiceReturnHeader H = new JobInvoiceReturnHeaderService(db).Find(temp.JobInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            JobInvoiceReturnLineViewModel temp = _JobInvoiceReturnLineService.GetJobInvoiceReturnLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PrepareViewBag(temp);

            JobInvoiceReturnHeader H = new JobInvoiceReturnHeaderService(db).Find(temp.JobInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobInvoiceReturnLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReturnDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobInvoiceReturnHeaderId, vm.JobInvoiceReturnLineId), ref db);
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

                int? StockId = 0;

                JobInvoiceReturnLine JobInvoiceReturnLine = db.JobInvoiceReturnLine.Find(vm.JobInvoiceReturnLineId);

                try
                {
                    JobInvoiceReturnDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobInvoiceReturnLine.JobInvoiceReturnHeaderId, JobInvoiceReturnLine.JobInvoiceReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobInvoiceReturnLine>(JobInvoiceReturnLine),
                });

                JobInvoiceReturnHeader header = new JobInvoiceReturnHeaderService(db).Find(JobInvoiceReturnLine.JobInvoiceReturnHeaderId);


                int? JobReturnLineId = JobInvoiceReturnLine.JobReturnLineId;

                var chargeslist = (from p in db.JobInvoiceReturnLineCharge
                                   where p.LineTableId == vm.JobInvoiceReturnLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobInvoiceReturnLineCharge.Remove(item);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = new JobInvoiceReturnHeaderChargeService(db).Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceReturnHeaderCharge.Add(footer);
                    }

                JobInvoiceReturnLine.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceReturnLine.Remove(JobInvoiceReturnLine);

                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;

                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceReturnHeader.Add(header);
                }

                if (JobReturnLineId != null)
                {
                    JobReturnLine Gline = db.JobReturnLine.Find((int)JobReturnLineId);
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReturnLine>(Gline),
                    });
                    JobReturnHeader Gheader = new JobReturnHeaderService(_unitOfWork).Find(Gline.JobReturnHeaderId);
                    new JobInvoiceLineStatusService(db).UpdateJobInvoiceQtyOnReturn(JobInvoiceReturnLine.JobInvoiceLineId, JobInvoiceReturnLine.JobInvoiceReturnLineId, header.DocDate, 0, 0, ref db);
                    StockId = Gline.StockId;

                    var JobReceiveLine = new JobReceiveLineService(_unitOfWork).Find(Gline.JobReceiveLineId);

                    if (JobReceiveLine.ProductUidId.HasValue)
                    {
                        //Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues(JobReceiveLine.ProductUidId.Value, "Job Return-" + vm.JobReturnHeaderId.ToString());

                        ProductUid ProductUid = new ProductUidService(_unitOfWork).Find(JobReceiveLine.ProductUidId.Value);

                        if (!(Gline.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && Gline.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || Gheader.SiteId == 17)
                        {

                            if (Gheader.DocNo != ProductUid.LastTransactionDocNo || Gheader.DocTypeId != ProductUid.LastTransactionDocTypeId)
                            {
                                ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                                PrepareViewBag(vm);
                                ViewBag.LineMode = "Delete";
                                return PartialView("_Create", vm);
                            }

                            ProductUid.LastTransactionDocDate = Gline.ProductUidLastTransactionDocDate;
                            ProductUid.LastTransactionDocId = Gline.ProductUidLastTransactionDocId;
                            ProductUid.LastTransactionDocNo = Gline.ProductUidLastTransactionDocNo;
                            ProductUid.LastTransactionDocTypeId = Gline.ProductUidLastTransactionDocTypeId;
                            ProductUid.LastTransactionPersonId = Gline.ProductUidLastTransactionPersonId;
                            ProductUid.CurrenctGodownId = Gline.ProductUidCurrentGodownId;
                            ProductUid.CurrenctProcessId = Gline.ProductUidCurrentProcessId;
                            ProductUid.Status = Gline.ProductUidStatus;

                            //new ProductUidService(_unitOfWork).Update(ProductUid);
                            ProductUid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(ProductUid);

                        }
                    }

                    Gline.ObjectState = Model.ObjectState.Deleted;
                    db.JobReturnLine.Remove(Gline);

                    if (StockId != null)
                    {
                        StockAdj Adj = (from L in db.StockAdj
                                        where L.StockOutId == StockId
                                        select L).FirstOrDefault();

                        if (Adj != null)
                        {
                            //new StockAdjService(_unitOfWork).Delete(Adj);
                            Adj.ObjectState = Model.ObjectState.Deleted;
                            db.StockAdj.Remove(Adj);
                        }

                        new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
                    }

                    Gheader.ModifiedBy = User.Identity.Name;
                    Gheader.ModifiedDate = DateTime.Now;
                    db.JobReturnHeader.Add(Gheader);
                }




                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

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
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobInvoiceReturnDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobInvoiceReturnLine.JobInvoiceReturnHeaderId, JobInvoiceReturnLine.JobInvoiceReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }
                //LogActivity.LogActivityDetail(header.DocTypeId,
                //header.JobInvoiceReturnHeaderId,
                //JobInvoiceReturnLine.JobInvoiceReturnLineId,
                //(int)ActivityTypeContants.Modified,
                //"",
                //User.Identity.Name, header.DocNo, Modifications);

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

        public JsonResult GetPendingInvoices(int JobInvoiceReturnHeaderId, string term, int Limit)
        {
            return Json(new JobInvoiceReturnHeaderService(db).GetPendingInvoices(JobInvoiceReturnHeaderId, term, Limit).ToList());
        }

        public JsonResult GetInvoiceDetail(int InvoiceLineId)
        {
            return Json(_JobInvoiceReturnLineService.GetJobInvoiceLineBalance(InvoiceLineId));
        }

        public JsonResult GetJobInvoices(int id, string term)//Receipt Header ID
        {
            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobInvoiceReturnLineService.GetPendingJobInvoiceHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJobReceives(int id, string term)//Receipt Header ID
        {
            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobInvoiceReturnLineService.GetPendingJobReceiveHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_JobInvoiceReturnLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBarCodes(string Id, int GodownId)
        {
            return Json(_JobInvoiceReturnLineService.GetPendingBarCodesList(Id, GodownId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductUidDetail(string ProductUidName, int filter)
        {

            var InvoiceRec = _JobInvoiceReturnLineService.GetProductUidDetail(ProductUidName, filter);

            if (InvoiceRec != null)
            {
                return Json(InvoiceRec);
            }
            else
            {
                return Json(new { Success = false });
            }
        }


        public ActionResult GetJobInvoiceForProduct(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _JobInvoiceReturnLineService.GetJobInvoiceHelpListForProduct(filter, searchTerm);
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

        public JsonResult SetSingleJobInvoiceLine(int Ids)
        {
            ComboBoxResult JobInvoiceJson = new ComboBoxResult();

            var JobInvoiceLine = from L in db.JobInvoiceLine
                                 join H in db.JobInvoiceHeader on L.JobInvoiceHeaderId equals H.JobInvoiceHeaderId into JobInvoiceHeaderTable
                                 from JobInvoiceHeaderTab in JobInvoiceHeaderTable.DefaultIfEmpty()
                                 where L.JobInvoiceLineId == Ids
                                 select new
                                 {
                                     JobInvoiceLineId = L.JobInvoiceLineId,
                                     JobInvoiceNo = L.JobReceiveLine.Product.ProductName
                                 };

            JobInvoiceJson.id = JobInvoiceLine.FirstOrDefault().ToString();
            JobInvoiceJson.text = JobInvoiceLine.FirstOrDefault().JobInvoiceNo;

            return Json(JobInvoiceJson);
        }

        public ActionResult GetCostCenterForPerson(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _JobInvoiceReturnLineService.GetCostCenterForPerson(filter, searchTerm);
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
