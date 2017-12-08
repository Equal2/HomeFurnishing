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
using JobInvoiceDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Jobs.Helpers;
using System.Data.Entity;
namespace Jobs.Controllers
{

    [Authorize]
    public class JobInvoiceLineController : System.Web.Mvc.Controller, IDisposable
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;
        IJobInvoiceLineService _JobInvoiceLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobInvoiceLineController(IJobInvoiceLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
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
           // return Json(p, JsonRequestBehavior.AllowGet);
            var p = Json(_JobInvoiceLineService.GetLineListForIndex(id).ToList(), JsonRequestBehavior.AllowGet);
            p.MaxJsonLength = int.MaxValue;
            return p;
        }
        public ActionResult _ForReceipt(int id, int? JobworkrId)
        {
            JobInvoiceLineFilterViewModel vm = new JobInvoiceLineFilterViewModel();

            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(id);

            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            if (JobworkrId.HasValue)
                vm.JobWorkerId = JobworkrId.Value;
            vm.JobInvoiceHeaderId = id;
            vm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettingsViewModel>(Settings);
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForOrder(int id, int? JobworkrId)
        {
            JobInvoiceLineFilterViewModel vm = new JobInvoiceLineFilterViewModel();

            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(id);

            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            if (JobworkrId.HasValue)
                vm.JobWorkerId = JobworkrId.Value;
            vm.JobInvoiceHeaderId = id;
            vm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettingsViewModel>(Settings);
            return PartialView("_OrderFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobInvoiceLineFilterViewModel vm)
        {
            List<JobInvoiceLineViewModel> temp = _JobInvoiceLineService.GetJobReceiptForFilters(vm).ToList();

            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceHeaderId);

            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            JobInvoiceMasterDetailModel svm = new JobInvoiceMasterDetailModel();
            svm.JobInvoiceLineViewModel = temp;
            svm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettingsViewModel>(Settings);
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(JobInvoiceLineFilterViewModel vm)
        {
            List<JobInvoiceLineViewModel> temp = _JobInvoiceLineService.GetJobOrderForFilters(vm).ToList();
            JobInvoiceMasterDetailModel svm = new JobInvoiceMasterDetailModel();
            svm.JobInvoiceLineViewModel = temp;
            return PartialView("_OrderResults", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobInvoiceMasterDetailModel vm)
        {
            List<JobInvoiceLine> Lines = new List<JobInvoiceLine>();
            List<JobInvoiceLineStatus> LineStat = new List<JobInvoiceLineStatus>();
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<LineReferenceIds> RefIds = new List<LineReferenceIds>();
            db.Configuration.AutoDetectChangesEnabled = false;

            int Serial = _JobInvoiceLineService.GetMaxSr(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId);
            bool HeaderChargeEdit = false;

            JobInvoiceHeader Header = new JobInvoiceHeaderService(_unitOfWork).Find(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId);

            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new JobInvoiceLineChargeService(_unitOfWork).GetMaxProductCharge(Header.JobInvoiceHeaderId, "Web.JobInvoiceLines", "JobInvoiceHeaderId", "JobInvoiceLineId");

            int PersonCount = 0;
            if (!Settings.CalculationId.HasValue)
            {
                throw new Exception("Calculation not configured in purchase order settings");
            }

            int CalculationId = Settings.CalculationId ?? 0;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId), ref db);
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
                try
                {
                    foreach (var item in vm.JobInvoiceLineViewModel)
                    {
                        if (item.DealQty > 0 && item.Rate > 0)
                        {
                            JobInvoiceLine line = new JobInvoiceLine();
                            //var receipt = new JobGoodsReceiptLineService(_unitOfWork).Find(item.JobGoodsReceiptLineId );
                            line.JobInvoiceHeaderId = item.JobInvoiceHeaderId;
                            line.JobReceiveLineId = item.JobReceiveLineId;
                            line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                            //line.Remark = receipt.Remark;
                            line.JobWorkerId = item.JobWorkerId;
                            line.Rate = item.Rate;
                            line.DealUnitId = item.DealUnitId;
                            line.Sr = Serial++;
                            line.Qty = item.ReceiptBalQty;
                            line.DealQty = item.DealQty;
                            line.Amount = DecimalRoundOff.amountToFixed((item.DealQty * item.Rate), Settings.AmountRoundOff);
                            line.CostCenterId = item.CostCenterId;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobInvoiceLineId = pk;

                            LineStatus.Add(line.JobReceiveLineId, 0);

                            line.ObjectState = Model.ObjectState.Added;
                            //_JobInvoiceLineService.Create(line);
                            Lines.Add(line);

                            JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                            Status.JobInvoiceLineId = line.JobInvoiceLineId;
                            Status.ObjectState = Model.ObjectState.Added;
                            LineStat.Add(Status);

                            JobReceiveLine Jobreceive = new JobReceiveLineService(_unitOfWork).Find(item.JobReceiveLineId);
                            Jobreceive.LockReason = "Job Invoice Completed";
                            Jobreceive.ObjectState = Model.ObjectState.Modified;
                            db.JobReceiveLine.Add(Jobreceive);


                            LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobInvoiceLineId, HeaderTableId = item.JobInvoiceHeaderId, PersonID = item.JobWorkerId, DealQty = line.DealQty, CostCenterId = line.CostCenterId });
                            RefIds.Add(new LineReferenceIds { LineId = line.JobInvoiceLineId, RefLineId = line.JobReceiveLineId });
                            pk++;



                        }
                    }

                    db.JobInvoiceLine.AddRange(Lines);
                    db.JobInvoiceLineStatus.AddRange(LineStat);

                    List<ReferenceLineChargeViewModel> temp = new List<ReferenceLineChargeViewModel>();

                    int[] RecLineIds = null;
                    RecLineIds = RefIds.Select(m => m.RefLineId).ToArray();


                    //temp = (from p in db.JobReceiveLine
                    //        where RecLineIds.Contains(p.JobReceiveLineId)
                    //        join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into JobOrderLineTable
                    //        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    //        join LineCharge in db.JobOrderLineCharge on p.JobOrderLineId equals LineCharge.LineTableId into JobOrderLineChargeTable
                    //        from JobOrderLineChargeTab in JobOrderLineChargeTable.DefaultIfEmpty()
                    //        join HeaderCharge in db.JobOrderHeaderCharges on JobOrderLineTab.JobOrderHeaderId equals HeaderCharge.HeaderTableId into JobOrderHeaderChargeTable
                    //        from JObOrderHeaderChargeTab in JobOrderHeaderChargeTable.DefaultIfEmpty()
                    //        join rqa in db.JobReceiveQALine on p.JobReceiveLineId equals rqa.JobReceiveLineId into qatable
                    //        from qatab in qatable.DefaultIfEmpty()
                    //        group new { p, JobOrderLineChargeTab, JObOrderHeaderChargeTab, qatab } by new { p.JobReceiveLineId } into g
                    //        select new ReferenceLineChargeViewModel
                    //        {
                    //            LineId = g.Key.JobReceiveLineId,
                    //            HeaderCharges = g.Select(m => m.JObOrderHeaderChargeTab).ToList(),
                    //            Linecharges = g.Select(m => m.JobOrderLineChargeTab).ToList(),
                    //            PenaltyAmt = g.Select(m => m.p.PenaltyAmt - (m.p.IncentiveAmt ?? 0)).FirstOrDefault() + ((g.Select(m => m.qatab).FirstOrDefault() == null) ? 0 : g.Select(m => m.qatab.PenaltyAmt).FirstOrDefault()),
                    //        }).ToList();

                    temp = (from p in db.JobReceiveLine
                            where RecLineIds.Contains(p.JobReceiveLineId)
                            join rqa in db.JobReceiveQALine on p.JobReceiveLineId equals rqa.JobReceiveLineId into qatable
                            from qatab in qatable.DefaultIfEmpty()
                            group new { p, qatab } by new { p.JobReceiveLineId } into g
                            select new ReferenceLineChargeViewModel
                            {
                                LineId = g.Key.JobReceiveLineId,
                                PenaltyAmt = g.Select(m => m.p.PenaltyAmt - (m.p.IncentiveAmt ?? 0)).FirstOrDefault() + ((g.Select(m => m.qatab).FirstOrDefault() == null) ? 0 : g.Select(m => m.qatab.PenaltyAmt).FirstOrDefault()),
                                ChargeGroupProductId = g.Select(m => m.p.JobOrderLine.Product.SalesTaxGroupProductId ?? m.p.JobOrderLine.Product.ProductGroup.DefaultSalesTaxGroupProductId).FirstOrDefault(),
                            }).ToList();



                    
                    //foreach (var item in temp)
                    //{
                    //    List<CalculationProductViewModel> ChargeRates = new List<CalculationProductViewModel>();
                    //    ChargeRates = new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId, Header.ProcessId, Header.SalesTaxGroupPersonId, item.ChargeGroupProductId).ToList();
                    //    List<CalculationProduct> LineChargeList = new List<CalculationProduct>();
                    //    LineChargeList = (from L in db.CalculationProduct where L.CalculationId == CalculationId select L).ToList();


                    //    foreach (var LineC in LineChargeList)
                    //    {
                    //        foreach (var Ct in ChargeRates)
                    //        {
                    //            if (LineC.ChargeId == Ct.ChargeId)
                    //            {
                    //                LineC.Rate = Ct.Rate;
                    //                LineC.LedgerAccountCrId = Ct.LedgerAccountCrId;
                    //                LineC.LedgerAccountDrId = Ct.LedgerAccountDrId;
                    //            }
                    //        }
                    //    }

                    //    item.Linecharges = new List<CalculationProduct>();
                    //    item.Linecharges.AddRange(LineChargeList);

                    //    List<CalculationFooter> HeaderChargeList = (from L in db.CalculationFooter where L.CalculationId == CalculationId select L).ToList();
                    //    item.HeaderCharges = HeaderChargeList;
                    //}

                    List<CalculationProduct> LineChargeListCollection = (from L in db.CalculationProduct where L.CalculationId == CalculationId select L).ToList();
                    foreach (var item in temp)
                    {
                        List<CalculationProductViewModel> ChargeRates = new List<CalculationProductViewModel>();
                        ChargeRates = new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId, Header.ProcessId, Header.SalesTaxGroupPersonId, item.ChargeGroupProductId).ToList();
                        //List<CalculationProduct> LineChargeList = new List<CalculationProduct>();
                        //LineChargeList = (from L in db.CalculationProduct where L.CalculationId == CalculationId select L).ToList();
                        item.Linecharges = new List<CalculationProduct>();

                        foreach (var LineC in LineChargeListCollection)
                        {
                            foreach (var Ct in ChargeRates)
                            {
                                if (LineC.ChargeId == Ct.ChargeId)
                                {
                                    CalculationProduct Cp = new CalculationProduct();
                                    Cp.CalculationProductId  =   LineC.CalculationProductId;
                                    Cp.CalculationId = LineC.CalculationId;
                                    Cp.Sr = LineC.Sr;
                                    Cp.ChargeId = LineC.ChargeId;
                                    Cp.AddDeduct = LineC.AddDeduct;
                                    Cp.AffectCost = LineC.AffectCost;
                                    Cp.ChargeTypeId = LineC.ChargeTypeId;
                                    Cp.CalculateOnId = LineC.CalculateOnId;
                                    Cp.CostCenterId = LineC.CostCenterId;
                                    Cp.RateType = LineC.RateType;
                                    Cp.IncludedInBase = LineC.IncludedInBase;
                                    Cp.ParentChargeId = LineC.ParentChargeId;
                                    Cp.Rate = Ct.Rate;
                                    Cp.Amount = LineC.Amount;
                                    Cp.RoundOff = LineC.RoundOff;
                                    Cp.LedgerAccountDrId = Ct.LedgerAccountDrId;
                                    Cp.LedgerAccountCrId = Ct.LedgerAccountCrId;
                                    Cp.PersonId = LineC.PersonId;
                                    Cp.IncludedCharges = LineC.IncludedCharges;
                                    Cp.IncludedChargesCalculation = LineC.IncludedChargesCalculation;
                                    Cp.IsVisible = LineC.IsVisible;
                                    Cp.IsActive = LineC.IsActive;
                                    Cp.CreatedBy = LineC.CreatedBy;
                                    Cp.ModifiedBy = LineC.ModifiedBy;
                                    Cp.CreatedDate = LineC.CreatedDate;
                                    Cp.ModifiedDate = LineC.ModifiedDate;



                                    //Cp.Rate = Ct.Rate;
                                    //Cp.LedgerAccountCrId = Ct.LedgerAccountCrId;
                                    //Cp.LedgerAccountDrId = Ct.LedgerAccountDrId;
                                    item.Linecharges.Add(Cp);
                                }
                            }
                        }


                        List<CalculationFooter> HeaderChargeList = (from L in db.CalculationFooter where L.CalculationId == CalculationId select L).ToList();
                        item.HeaderCharges = HeaderChargeList;
                    }




                    //for (int i = 0; i <= temp.Count - 1; i++)
                    //{
                    //    List<CalculationProductViewModel> ChargeRates = new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId, Header.ProcessId, Header.SalesTaxGroupPersonId, temp[i].ChargeGroupProductId).ToList();
                    //    temp[i].Linecharges = (from L in db.CalculationProduct where L.CalculationId == CalculationId select L).ToList();

                    //    for (int j = 0; j <= temp[i].Linecharges.Count - 1; j++)
                    //    {
                    //        for (int k = 0; k <= ChargeRates.Count() - 1; k++)
                    //        {
                    //            if (temp[i].Linecharges[j].ChargeId == ChargeRates[k].ChargeId)
                    //            {
                    //                temp[i].Linecharges[j].Rate = ChargeRates[k].Rate;
                    //                temp[i].Linecharges[j].LedgerAccountCrId = ChargeRates[k].LedgerAccountCrId;
                    //                temp[i].Linecharges[j].LedgerAccountDrId = ChargeRates[k].LedgerAccountDrId;
                    //            }
                    //        }
                    //    }


                    //    List<CalculationFooter> HeaderChargeList = (from L in db.CalculationFooter where L.CalculationId == CalculationId select L).ToList();
                    //    temp[i].HeaderCharges = HeaderChargeList;
                    //}




                    //IEnumerable<JobOrderLineCharge> SettingLineCharges = (from L in db.JobReceiveLine
                    //                                                      join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                    //                                                      from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                    //                                                      join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                    //                                                      from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    //                                                      join Prod in db.Product on L.ProductId equals Prod.ProductId into ProductTable
                    //                                                      from ProductTab in ProductTable.DefaultIfEmpty()
                    //                                                      join Cgs in db.ChargeGroupSettings
                    //                                                           on new { A1 = ProductTab.SalesTaxGroupProductId ?? 0, A2 = JobOrderLineTab.JobOrderHeader.SalesTaxGroupPersonId ?? 0, JobReceiveHeaderTab.ProcessId }
                    //                                                           equals new { A1 = Cgs.ChargeGroupProductId, A2 = Cgs.ChargeGroupPersonId, Cgs.ProcessId } into ChargeGroupSettingsTable
                    //                                                      from ChargeGroupSettingsTab in ChargeGroupSettingsTable.DefaultIfEmpty()
                    //                                                      join LineCharge in db.JobOrderLineCharge on L.JobOrderLineId equals LineCharge.LineTableId into JobOrderLineChargeTable
                    //                                                      from JobOrderLineChargeTab in JobOrderLineChargeTable.DefaultIfEmpty()
                    //                                                      join Cp in db.CalculationProduct.Where(m => m.CalculationId == CalculationId) on JobOrderLineChargeTab.ChargeId equals Cp.ChargeId into CalculationProductTable
                    //                                                      from CalculationProductTab in CalculationProductTable.DefaultIfEmpty()
                    //                                                      where RecLineIds.Contains(L.JobReceiveLineId) && CalculationProductTab.CalculationProductId != null
                    //                                                      select new
                    //                                                      {
                    //                                                          Id = JobOrderLineChargeTab.Id,
                    //                                                          HeaderTableId = JobOrderLineChargeTab.HeaderTableId,
                    //                                                          LineTableId = JobOrderLineChargeTab.LineTableId,
                    //                                                          Sr = CalculationProductTab.Sr,
                    //                                                          ChargeId = CalculationProductTab.ChargeId,
                    //                                                          AddDeduct = CalculationProductTab.AddDeduct,
                    //                                                          AffectCost = CalculationProductTab.AffectCost,
                    //                                                          ChargeTypeId = CalculationProductTab.ChargeTypeId,
                    //                                                          CalculateOnId = CalculationProductTab.CalculateOnId,
                    //                                                          PersonID = JobReceiveHeaderTab.JobWorkerId,
                    //                                                          LedgerAccountDrId = CalculationProductTab.LedgerAccountDrId,
                    //                                                          LedgerAccountCrId = CalculationProductTab.LedgerAccountCrId,
                    //                                                          CostCenterId = CalculationProductTab.CostCenterId,
                    //                                                          RateType = CalculationProductTab.RateType,
                    //                                                          IncludedInBase = CalculationProductTab.IncludedInBase,
                    //                                                          ParentChargeId = CalculationProductTab.ParentChargeId,
                    //                                                          Rate = (Decimal?)ChargeGroupSettingsTab.ChargePer ?? 0,
                    //                                                          Amount = (Decimal?)CalculationProductTab.Amount ?? 0,
                    //                                                          DealQty = JobOrderLineTab.DealQty,
                    //                                                          IsVisible = CalculationProductTab.IsVisible,
                    //                                                          IncludedCharges = CalculationProductTab.IncludedCharges,
                    //                                                          IncludedChargesCalculation = CalculationProductTab.IncludedChargesCalculation
                    //                                                      }).AsEnumerable().Select(x => new JobOrderLineCharge
                    //                          {
                    //                              Id = x.Id,
                    //                              HeaderTableId = x.HeaderTableId,
                    //                              LineTableId = x.LineTableId,
                    //                              Sr = x.Sr,
                    //                              ChargeId = x.ChargeId,
                    //                              AddDeduct = x.AddDeduct,
                    //                              AffectCost = x.AffectCost,
                    //                              ChargeTypeId = x.ChargeTypeId,
                    //                              CalculateOnId = x.CalculateOnId,
                    //                              PersonID = x.PersonID,
                    //                              LedgerAccountDrId = x.LedgerAccountDrId,
                    //                              LedgerAccountCrId = x.LedgerAccountCrId,
                    //                              CostCenterId = x.CostCenterId,
                    //                              RateType = x.RateType,
                    //                              IncludedInBase = x.IncludedInBase,
                    //                              ParentChargeId = x.ParentChargeId,
                    //                              Rate = x.Rate,
                    //                              Amount = x.Amount,
                    //                              DealQty = x.DealQty,
                    //                              IsVisible = x.IsVisible,
                    //                              IncludedCharges = x.IncludedCharges,
                    //                              IncludedChargesCalculation = x.IncludedChargesCalculation
                    //                          }).ToList();






                    //IEnumerable<JobOrderLineCharge> SettingLineCharges123 = (from L in db.JobReceiveLine
                    //                                                      join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                    //                                                      from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                    //                                                      join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                    //                                                      from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    //                                                      join Prod in db.Product on L.ProductId equals Prod.ProductId into ProductTable
                    //                                                      from ProductTab in ProductTable.DefaultIfEmpty()
                    //                                                      join Cgs in db.ChargeGroupSettings
                    //                                                           on new { A1 = ProductTab.SalesTaxGroupProductId ?? 0, A2 = JobOrderLineTab.JobOrderHeader.SalesTaxGroupPersonId ?? 0, JobReceiveHeaderTab.ProcessId }
                    //                                                           equals new { A1 = Cgs.ChargeGroupProductId, A2 = Cgs.ChargeGroupPersonId, Cgs.ProcessId } into ChargeGroupSettingsTable
                    //                                                      from ChargeGroupSettingsTab in ChargeGroupSettingsTable.DefaultIfEmpty()
                    //                                                      join LineCharge in db.JobOrderLineCharge on L.JobOrderLineId equals LineCharge.LineTableId into JobOrderLineChargeTable
                    //                                                      from JobOrderLineChargeTab in JobOrderLineChargeTable.DefaultIfEmpty()
                    //                                                      join Cp in db.CalculationProduct.Where(m => m.CalculationId == CalculationId) on JobOrderLineChargeTab.ChargeId equals Cp.ChargeId into CalculationProductTable
                    //                                                      from CalculationProductTab in CalculationProductTable.DefaultIfEmpty()
                    //                                                      where RecLineIds.Contains(L.JobReceiveLineId) && CalculationProductTab.CalculationProductId != null
                    //                                                      select JobOrderLineChargeTab);

                    //temp = (from p in db.JobReceiveLine
                    //        where RecLineIds.Contains(p.JobReceiveLineId)
                    //        join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId into JobOrderLineTable
                    //        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                    //        join LineCharge in db.JobOrderLineCharge on p.JobOrderLineId equals LineCharge.LineTableId into JobOrderLineChargeTable
                    //        from JobOrderLineChargeTab in JobOrderLineChargeTable.DefaultIfEmpty()
                    //        join HeaderCharge in db.JobOrderHeaderCharges on JobOrderLineTab.JobOrderHeaderId equals HeaderCharge.HeaderTableId into JobOrderHeaderChargeTable
                    //        from JObOrderHeaderChargeTab in JobOrderHeaderChargeTable.DefaultIfEmpty()
                    //        join rqa in db.JobReceiveQALine on p.JobReceiveLineId equals rqa.JobReceiveLineId into qatable
                    //        from qatab in qatable.DefaultIfEmpty()
                    //        group new { p, JobOrderLineChargeTab, JObOrderHeaderChargeTab, qatab } by new { p.JobReceiveLineId } into g
                    //        select new ReferenceLineChargeViewModel
                    //        {
                    //            LineId = g.Key.JobReceiveLineId,
                    //            HeaderCharges = g.Select(m => m.JObOrderHeaderChargeTab).ToList(),
                    //            Linecharges = g.Select(m => m.JobOrderLineChargeTab).ToList(),
                    //            PenaltyAmt = g.Select(m => m.p.PenaltyAmt - (m.p.IncentiveAmt ?? 0)).FirstOrDefault() + ((g.Select(m => m.qatab).FirstOrDefault() == null) ? 0 : g.Select(m => m.qatab.PenaltyAmt).FirstOrDefault()),
                    //        }).ToList();




                    var LineListWithReferences = (from p in LineList
                                                  join t in RefIds on p.LineTableId equals t.LineId
                                                  join t2 in temp on t.RefLineId equals t2.LineId into table
                                                  from LineLis in table.DefaultIfEmpty()
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
                                                      Penalty = LineLis == null ? 0 : LineLis.PenaltyAmt,
                                                      RLineCharges = (LineLis == null || LineLis.Linecharges.FirstOrDefault() == null ? null : Mapper.Map<List<LineChargeViewModel>>(LineLis.Linecharges.GroupBy(m => m.CalculationProductId).Select(m => m.FirstOrDefault()))),
                                                  }).ToList();


                    new ChargesCalculationService(_unitOfWork).CalculateCharges(LineListWithReferences, vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceHeaderCharges", "Web.JobInvoiceLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                    //Calculate Charges::::::
                    //CalculateCharges(LineList, vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceHeaderCharges", "Web.JobInvoiceLineCharges");

                    List<JobInvoiceLineCharge> LineChgs = new List<JobInvoiceLineCharge>();

                    //Saving Charges
                    foreach (var item in LineCharges)
                    {

                        JobInvoiceLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobInvoiceLineCharge>(item);
                        PoLineCharge.ObjectState = Model.ObjectState.Added;
                        //new JobInvoiceLineChargeService(_unitOfWork).Create(PoLineCharge);
                        LineChgs.Add(PoLineCharge);

                    }

                    db.JobInvoiceLineCharge.AddRange(LineChgs);

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
                            // new JobInvoiceHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                            db.JobInvoiceHeaderCharges.Add(POHeaderCharge);
                        }
                        else
                        {
                            var footercharge = new JobInvoiceHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                            footercharge.Rate = HeaderCharges[i].Rate;
                            footercharge.Amount = HeaderCharges[i].Amount;
                            if (PersonCount > 1 || footercharge.PersonID != vm.JobInvoiceLineViewModel.FirstOrDefault().JobWorkerId)
                                footercharge.PersonID = null;
                            //new JobInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceHeaderCharges.Add(footercharge);
                        }

                    }

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyInvoiceMultiple(LineStatus, Header.DocDate, ref db);
                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyInvoiceMultiple(LineStatus, Header.DocDate, ref db);

                    if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                    {
                        Header.Status = (int)StatusConstants.Modified;
                        Header.ModifiedDate = DateTime.Now;
                        Header.ModifiedBy = User.Identity.Name;
                        Header.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceHeader.Add(Header);
                    }

                    try
                    {
                        JobInvoiceDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }


                    if (EventException)
                    { throw new Exception(); }
                    //_unitOfWork.Save();
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                try
                {
                    JobInvoiceDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceLineViewModel.FirstOrDefault().JobInvoiceHeaderId), ref db);
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
                    DocStatus = Header.Status
                }));


                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(JobInvoiceLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }


        [HttpGet]
        public ActionResult CreateLine(int Id, int? JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int? JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int? JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }


        public ActionResult _Create(int Id, int? JobWorkerId) //Id ==>Job Invoice Header Id
        {
            JobInvoiceHeader H = new JobInvoiceHeaderService(_unitOfWork).Find(Id);
            JobInvoiceLineViewModel s = new JobInvoiceLineViewModel();

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);


            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;
            s.SalesTaxGroupPersonId = H.SalesTaxGroupPersonId;

            if (JobWorkerId.HasValue)
                s.JobWorkerId = JobWorkerId.Value;

            s.JobInvoiceHeaderId = H.JobInvoiceHeaderId;
            PrepareViewBag(null);
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }

            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobInvoiceLineViewModel svm)
        {
            JobInvoiceLine s = Mapper.Map<JobInvoiceLineViewModel, JobInvoiceLine>(svm);
            JobReceiveLine Rec = new JobReceiveLineService(_unitOfWork).Find(s.JobReceiveLineId);
            JobInvoiceHeader temp = new JobInvoiceHeaderService(_unitOfWork).Find(s.JobInvoiceHeaderId);
            bool BeforeSave = true;

            if (svm.JobReceiveLineId <= 0)
            {
                ModelState.AddModelError("", "Job Receive field is required");
            }

            if (svm.DealQty <= 0)
            {
                ModelState.AddModelError("", "DealQty field is required");
            }
            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("", "Qty field is required");
            }


            #region "Tax Calculation Validation"
            //SiteDivisionSettings SiteDivisionSettings = new SiteDivisionSettingsService(_unitOfWork).GetSiteDivisionSettings(temp.SiteId, temp.DivisionId, temp.DocDate);
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

            //        if (svm.SalesTaxGroupProductId != 0 && svm.SalesTaxGroupProductId != null && svm.SalesTaxGroupPersonId != 0 && svm.SalesTaxGroupPersonId != null && svm.JobInvoiceSettings.CalculationId != null)
            //        {
            //            IEnumerable<ChargeRateSettings> ChargeRateSettingsList = new CalculationProductService(_unitOfWork).GetChargeRateSettingForValidation((int)svm.JobInvoiceSettings.CalculationId, temp.DocTypeId, temp.SiteId, temp.DivisionId, temp.ProcessId, (int)svm.SalesTaxGroupPersonId, (int)svm.SalesTaxGroupProductId);

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



            if (svm.JobReceiveLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            #region BeforeSave
            try
            {

                if (svm.JobOrderLineId <= 0)
                    BeforeSave = JobInvoiceDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceHeaderId, EventModeConstants.Edit), ref db);

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


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.JobInvoiceLineId <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.Sr = _JobInvoiceLineService.GetMaxSr(s.JobInvoiceHeaderId);
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoice(s.JobReceiveLineId, s.JobInvoiceLineId, temp.DocDate, Rec.Qty, s.UnitConversionMultiplier, ref db, true);
                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnInvoice(s.JobReceiveLineId, s.JobInvoiceLineId, temp.DocDate, Rec.Qty, ref db, true);

                    //_JobInvoiceLineService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceLine.Add(s);

                    Rec.LockReason = "Job Invoice Completed";
                    Rec.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveLine.Add(Rec);

                    JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                    Status.JobInvoiceLineId = s.JobInvoiceLineId;
                    Status.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceLineStatus.Add(Status);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.JobInvoiceLineId;
                            item.PersonID = s.JobWorkerId;
                            item.HeaderTableId = s.JobInvoiceHeaderId;
                            item.DealQty = s.DealQty;
                            item.CostCenterId = s.CostCenterId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.JobInvoiceLineCharge.Add(item);
                            //new JobInvoiceLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                    {
                        int PersonCount = (from p in db.JobInvoiceLine
                                           where p.JobInvoiceHeaderId == s.JobInvoiceHeaderId
                                           group p by p.JobWorkerId into g
                                           select g).Count();

                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {


                                var footercharge = new JobInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                                if (PersonCount > 1 || footercharge.PersonID != s.JobWorkerId)
                                    footercharge.PersonID = null;

                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.JobInvoiceHeaderCharges.Add(footercharge);
                                //new JobInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.JobInvoiceHeaderId;
                                item.PersonID = s.JobWorkerId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.JobInvoiceHeaderCharges.Add(item);
                                //new JobInvoiceHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }

                    }

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ObjectState = Model.ObjectState.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        db.JobInvoiceHeader.Add(temp);
                        //new JobInvoiceHeaderService(_unitOfWork).Update(temp);
                    }


                    try
                    {
                        JobInvoiceDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobInvoiceHeaderId, s.JobInvoiceLineId, EventModeConstants.Add), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobInvoiceDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobInvoiceHeaderId, s.JobInvoiceLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceHeaderId,
                        DocLineId = s.JobInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.JobInvoiceHeaderId, JobWorkerId = s.JobWorkerId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    JobInvoiceHeader header = new JobInvoiceHeaderService(_unitOfWork).Find(svm.JobInvoiceHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    JobInvoiceLine temp1 = _JobInvoiceLineService.Find(svm.JobInvoiceLineId);
                    JobInvoiceLine ExRec = new JobInvoiceLine();
                    ExRec = Mapper.Map<JobInvoiceLine>(temp1);

                    temp1.Amount = svm.Amount;
                    temp1.JobReceiveLineId = svm.JobReceiveLineId;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.SalesTaxGroupProductId = svm.SalesTaxGroupProductId;
                    temp1.DealQty = svm.DealQty;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.Remark = svm.Remark;
                    temp1.Rate = svm.Rate;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoice(s.JobReceiveLineId, s.JobInvoiceLineId, temp.DocDate, Rec.Qty, temp1.UnitConversionMultiplier, ref db, true);
                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnInvoice(s.JobReceiveLineId, s.JobInvoiceLineId, temp.DocDate, Rec.Qty, ref db, true);

                    temp1.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceLine.Add(temp1);
                    //_JobInvoiceLineService.Update(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ObjectState = Model.ObjectState.Modified;
                        header.ModifiedDate = DateTime.Now;
                        header.ModifiedBy = User.Identity.Name;
                        db.JobInvoiceHeader.Add(header);
                        //new JobInvoiceHeaderService(_unitOfWork).Update(header);
                    }

                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new JobInvoiceLineChargeService(_unitOfWork).Find(item.Id);

                            JobInvoiceLineCharge ExRecC = new JobInvoiceLineCharge();
                            ExRecC = Mapper.Map<JobInvoiceLineCharge>(productcharge);

                            productcharge.Rate = item.Rate ?? 0;
                            productcharge.Amount = item.Amount ?? 0;
                            productcharge.DealQty = temp1.DealQty;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                             db.JobInvoiceLineCharge.Add(productcharge);
                            //new JobInvoiceLineChargeService(_unitOfWork).Update(productcharge);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecC,
                                Obj = productcharge,
                            });
                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new JobInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);

                            JobInvoiceHeaderCharge ExRecC = new JobInvoiceHeaderCharge();
                            ExRecC = Mapper.Map<JobInvoiceHeaderCharge>(footercharge);

                            footercharge.Rate = item.Rate ?? 0;
                            footercharge.Amount = item.Amount ?? 0;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceHeaderCharges.Add(footercharge);
                            //new JobInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecC,
                                Obj = footercharge,
                            });
                        }


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobInvoiceHeaderId, temp1.JobInvoiceLineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobInvoiceDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobInvoiceHeaderId, temp1.JobInvoiceLineId, EventModeConstants.Edit), ref db);
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
                        DocId = header.JobInvoiceHeaderId,
                        DocLineId = temp1.JobInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        xEModifications = Modifications,
                        DocStatus = header.Status,
                    }));

                    //End of Saving the Activity Log

                    return Json(new { success = true });

                }
            }
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
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        private ActionResult _Modify(int id)
        {
            JobInvoiceLineViewModel temp = _JobInvoiceLineService.GetJobInvoiceLine(id);

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

            PrepareViewBag(temp);

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
            JobInvoiceLineViewModel temp = _JobInvoiceLineService.GetJobInvoiceLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }
            JobInvoiceHeader H = new JobInvoiceHeaderService(_unitOfWork).Find(temp.JobInvoiceHeaderId);
            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

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

            return PartialView("_Create", temp);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(JobInvoiceLineViewModel vm)
        {

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobInvoiceHeaderId, vm.JobInvoiceLineId), ref db);
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


                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                //JobInvoiceLine JobInvoiceLine = _JobInvoiceLineService.Find(vm.JobInvoiceLineId);
                JobInvoiceLine JobInvoiceLine = (from p in db.JobInvoiceLine
                                                 where p.JobInvoiceLineId == vm.JobInvoiceLineId
                                                 select p).FirstOrDefault();
                
                //JobReceiveLine Rec = (from p in db.JobReceiveLine
                //                      where p.JobReceiveLineId == JobInvoiceLine.JobReceiveLineId
                //                      select p).FirstOrDefault();

                JobInvoiceHeader header = new JobInvoiceHeaderService(_unitOfWork).Find(JobInvoiceLine.JobInvoiceHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobInvoiceLine>(JobInvoiceLine),
                });

                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnInvoice(JobInvoiceLine.JobReceiveLineId, JobInvoiceLine.JobInvoiceLineId, header.DocDate, 0, JobInvoiceLine.UnitConversionMultiplier, ref db, true);
                //new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnInvoice(JobInvoiceLine.JobReceiveLineId, JobInvoiceLine.JobInvoiceLineId, header.DocDate, 0, ref db, true);

                //JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                //Status.JobInvoiceLineId = JobInvoiceLine.JobInvoiceLineId;
                //db.JobInvoiceLineStatus.Attach(Status);
                //Status.ObjectState = Model.ObjectState.Deleted;
                //db.JobInvoiceLineStatus.Remove(Status);

                JobInvoiceLineStatus Status = db.JobInvoiceLineStatus.Find(JobInvoiceLine.JobInvoiceLineId);
                Status.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceLineStatus.Remove(Status);


                //_JobInvoiceLineService.Delete(JobInvoiceLine);
                //Rec.LockReason =null;
                //Rec.ObjectState = Model.ObjectState.Modified;
                //db.JobReceiveLine.Add(Rec);


                JobInvoiceLine.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceLine.Remove(JobInvoiceLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ObjectState = Model.ObjectState.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    db.JobInvoiceHeader.Add(header);
                    //new JobInvoiceHeaderService(_unitOfWork).Update(header);
                }



                var chargeslist = (from p in db.JobInvoiceLineCharge 
                                   where p.LineTableId == vm.JobInvoiceLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobInvoiceLineCharge.Remove(item);

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
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try 
                {
                    JobInvoiceDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobInvoiceLine.JobInvoiceHeaderId, JobInvoiceLine.JobInvoiceLineId), ref db);
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
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    PrepareViewBag(null);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobInvoiceDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobInvoiceLine.JobInvoiceHeaderId, JobInvoiceLine.JobInvoiceLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.JobInvoiceHeaderId,
                    DocLineId = JobInvoiceLine.JobInvoiceLineId,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            TempData["CSEXC"] = null;

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            ProductJson.Add(new Product()
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId
            });

            return Json(ProductJson);
        }

        public JsonResult GetPendingReceipts(int ProductId, int JobWorkerId)
        {
            return Json(new JobReceiveHeaderService(_unitOfWork).GetPendingReceipts(ProductId, JobWorkerId).ToList());
        }

        public JsonResult GetReceiptDetail(int ReceiptId)
        {
            return Json(new JobReceiveLineService(_unitOfWork).GetJobReceiveDetailBalanceForInvoice(ReceiptId));
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


        public JsonResult GetPendingJobReceiveProducts(int id, string term)//DocTypeId
        {
            return Json(_JobInvoiceLineService.GetPendingProductsForJobInvoice(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobReceive(int id, string term)//DocTypeId
        {
            return Json(_JobInvoiceLineService.GetPendingJobReceive(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobOrders(int id, string term)//DocTypeId
        {
            return Json(_JobInvoiceLineService.GetPendingJobOrders(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobWorkers(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _JobInvoiceLineService.GetPendingJobWorkersForJobInvoice(filter, searchTerm);

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

            // return Json(_JobInvoiceLineService.GetPendingJobWorkersForJobInvoice(filter, searchTerm), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, int? JobWorkerId, string term, int Limit)//Indent Header ID
        {
            return Json(_JobInvoiceLineService.GetProductHelpList(id, JobWorkerId, term, Limit), JsonRequestBehavior.AllowGet);
        }
    }
}
