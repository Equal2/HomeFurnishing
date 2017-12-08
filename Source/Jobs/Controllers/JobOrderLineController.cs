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
using JobOrderDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobOrderLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobOrderLineService _JobOrderLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public JobOrderLineController(IJobOrderLineService JobOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderLineService = JobOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _ForProdOrder(int id, int jid)
        {
            JobOrderLineFilterViewModel vm = new JobOrderLineFilterViewModel();

            JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(id);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(Settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            vm.DealUnitId = Settings.DealUnitId;
            vm.JobOrderHeaderId = id;
            vm.JobWorkerId = jid;
            PrepareViewBag(null);
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForStockIn(int id, int jid)
        {
            JobOrderLineFilterForStockInViewModel vm = new JobOrderLineFilterForStockInViewModel();
            JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(id);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(Settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            vm.DealUnitId = Settings.DealUnitId;
            vm.JobOrderHeaderId = id;
            vm.JobWorkerId = jid;
            PrepareViewBag(null);
            return PartialView("_FiltersStockIn", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderLineFilterViewModel vm)
        {

            //if (vm.JobOrderSettings.isVisibleRate && vm.JobOrderSettings.isMandatoryRate && (vm.Rate == null || vm.Rate == 0))
            //{
            //    ModelState.AddModelError("", "Rate is mandatory");
            //    PrepareViewBag(null);
            //    return PartialView("_Filters", vm);
            //}


            List<JobOrderLineViewModel> temp = _JobOrderLineService.GetProdOrdersForFilters(vm).ToList();

            bool UnitConvetsionException = (from p in temp
                                            where p.UnitConversionException == true
                                            select p).Any();

            if (UnitConvetsionException)
            {
                ModelState.AddModelError("", "Unit Conversion are missing for few Products");
            }

            JobOrderMasterDetailModel svm = new JobOrderMasterDetailModel();
            svm.JobOrderLineViewModel = temp;

            JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderHeaderId);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            svm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(Settings);

            if (svm.JobOrderSettings.isVisibleDealUnit == false && svm.JobOrderSettings.isVisibleLoss == false && svm.JobOrderSettings.isVisibleUncountableQty == false && svm.JobOrderSettings.isVisibleRate == true)
            {
                return PartialView("_ResultsWithRate", svm);
            }
            if (svm.JobOrderSettings.isVisibleDealUnit == true && svm.JobOrderSettings.isVisibleLoss == true && svm.JobOrderSettings.isVisibleUncountableQty == false && svm.JobOrderSettings.isVisibleRate == true)
            {
                return PartialView("_ResultsWithRateDealQtyLoss", svm);
            }
            if (svm.JobOrderSettings.isVisibleDealUnit == false && svm.JobOrderSettings.isVisibleLoss == true && svm.JobOrderSettings.isVisibleUncountableQty == false && svm.JobOrderSettings.isVisibleRate == true)
            {
                return PartialView("_ResultsWithRateLoss", svm);
            }
            if (svm.JobOrderSettings.isVisibleRate == false)
            {
                return PartialView("_ResultsWithQty", svm);
            }
            else
            {
                return PartialView("_Results", svm);
            }
        }


        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostStockIn(JobOrderLineFilterForStockInViewModel vm)
        {
            List<JobOrderLineViewModel> temp = _JobOrderLineService.GetJobOrderForStockInFilters(vm).ToList();

            bool UnitConvetsionException = (from p in temp
                                            where p.UnitConversionException == true
                                            select p).Any();

            if (UnitConvetsionException)
            {
                ModelState.AddModelError("", "Unit Conversion are missing for few Products");
            }

            JobOrderMasterDetailModel svm = new JobOrderMasterDetailModel();
            svm.JobOrderLineViewModel = temp;

            JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderHeaderId);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);

            svm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(Settings);

            if (svm.JobOrderSettings.isVisibleDealUnit == false && svm.JobOrderSettings.isVisibleLoss == false && svm.JobOrderSettings.isVisibleUncountableQty == false && svm.JobOrderSettings.isVisibleRate == true)
            {
                return PartialView("_ResultsWithRate", svm);
            }
            if (svm.JobOrderSettings.isVisibleDealUnit == true && svm.JobOrderSettings.isVisibleLoss == true && svm.JobOrderSettings.isVisibleUncountableQty == false && svm.JobOrderSettings.isVisibleRate == true)
            {
                return PartialView("_ResultsWithRateDealQtyLoss", svm);
            }
            if (svm.JobOrderSettings.isVisibleDealUnit == false && svm.JobOrderSettings.isVisibleLoss == true && svm.JobOrderSettings.isVisibleUncountableQty == false && svm.JobOrderSettings.isVisibleRate == true)
            {
                return PartialView("_ResultsWithRateLoss", svm);
            }
            if (svm.JobOrderSettings.isVisibleRate == false)
            {
                return PartialView("_ResultsWithQty", svm);
            }
            else
            {
                return PartialView("_Results", svm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobOrderLineService.GetMaxSr(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId);
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobOrderLine> BarCodesPendingToUpdate = new List<JobOrderLine>();
            List<LineChargeRates> LineChargeRates = new List<LineChargeRates>();

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId), ref db);
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

            JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new JobOrderLineChargeService(_unitOfWork).GetMaxProductCharge(Header.JobOrderHeaderId, "Web.JobOrderLines", "JobOrderHeaderId", "JobOrderLineId");

            int PersonCount = 0;

            decimal Qty = vm.JobOrderLineViewModel.Where(m => m.Rate > 0).Sum(m => m.Qty);

            //int CalculationId = Settings.CalculationId ?? 0;
            int CalculationId = 0;

            if (Header.SalesTaxGroupPersonId != null)
                CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(Header.DocTypeId, (int)Header.SalesTaxGroupPersonId, Header.SiteId, Header.DivisionId) ?? 0;

            if (CalculationId == 0)
                CalculationId = Settings.CalculationId ?? 0;



            //List<string> uids = new List<string>();

            //if (!string.IsNullOrEmpty(Settings.SqlProcGenProductUID))
            //{
            //    uids = _JobOrderLineService.GetProcGenProductUids(Header.DocTypeId, Qty, Header.DivisionId, Header.SiteId);
            //}

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            int RowNo = 0;
            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.JobOrderLineViewModel)
                {
                    RowNo = RowNo + 1;
                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                    //if (item.Qty > 0  && ((Settings.isVisibleRate == true && Settings.isMandatoryRate == true && item.Rate > 0) || (Settings.isVisibleRate == false || Settings.isVisibleRate == true && Settings.isMandatoryRate == false)))
                    if (item.Qty > 0)
                    {
                        if (Settings.isVisibleRate == true && Settings.isMandatoryRate == true && item.Rate == 0)
                        {
                            //string message = "Rate is not entered in line no" + RowNo.ToString();
                            //TempData["CSEXCL"] += message;
                            //EventException = true;

                            string msg = "Rate is not entered in line no" + RowNo.ToString();
                            ModelState.AddModelError("", msg);
                            return PartialView("_Results", vm);
                        }

                        if (item.UnitConversionMultiplier == 0 ||item.UnitConversionMultiplier == null)
                        {
                            item.UnitConversionMultiplier = 1;
                        }

                        JobOrderLine line = new JobOrderLine();

                        if (Settings.isPostedInStock ?? false)
                        {
                            StockViewModel StockViewModel = new StockViewModel();

                            if (Cnt == 0)
                            {
                                StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                            }
                            else
                            {
                                if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                                {
                                    StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                                }
                                else
                                {
                                    StockViewModel.StockHeaderId = -1;
                                }
                            }

                            StockViewModel.StockId = -Cnt;
                            StockViewModel.DocHeaderId = Header.JobOrderHeaderId;
                            StockViewModel.DocLineId = line.JobOrderLineId;
                            StockViewModel.DocTypeId = Header.DocTypeId;
                            StockViewModel.StockHeaderDocDate = Header.DocDate;
                            StockViewModel.StockDocDate = Header.DocDate;
                            StockViewModel.DocNo = Header.DocNo;
                            StockViewModel.DivisionId = Header.DivisionId;
                            StockViewModel.SiteId = Header.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.PersonId = Header.JobWorkerId;
                            StockViewModel.ProductId = item.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = Header.GodownId;
                            StockViewModel.HeaderProcessId = Header.ProcessId;
                            StockViewModel.GodownId = (int)Header.GodownId;
                            StockViewModel.Remark = Header.Remark;
                            StockViewModel.Status = Header.Status;
                            StockViewModel.ProcessId = item.FromProcessId;
                            StockViewModel.LotNo = item.LotNo;
                            StockViewModel.CostCenterId = Header.CostCenterId;
                            StockViewModel.Qty_Iss = item.Qty;
                            StockViewModel.Qty_Rec = 0;
                            StockViewModel.Rate = item.Rate;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = item.Specification;
                            StockViewModel.Dimension1Id = item.Dimension1Id;
                            StockViewModel.Dimension2Id = item.Dimension2Id;
                            StockViewModel.Dimension3Id = item.Dimension3Id;
                            StockViewModel.Dimension4Id = item.Dimension4Id;
                            StockViewModel.ProductUidId = item.ProductUidId;
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
                                Header.StockHeaderId = StockViewModel.StockHeaderId;
                            }
                            line.StockId = StockViewModel.StockId;
                        }



                        if (Settings.isPostedInStockProcess ?? false)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
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
                            StockProcessViewModel.DocHeaderId = Header.JobOrderHeaderId;
                            StockProcessViewModel.DocLineId = line.JobOrderLineId;
                            StockProcessViewModel.DocTypeId = Header.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                            StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                            StockProcessViewModel.DocNo = Header.DocNo;
                            StockProcessViewModel.DivisionId = Header.DivisionId;
                            StockProcessViewModel.SiteId = Header.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = Header.JobWorkerId;
                            StockProcessViewModel.ProductId = item.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = Header.GodownId;
                            StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                            StockProcessViewModel.GodownId = (int)Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = item.LotNo;
                            StockProcessViewModel.CostCenterId = Header.CostCenterId;
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = item.Qty;
                            StockProcessViewModel.Rate = item.Rate;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = item.Specification;
                            StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = item.Dimension4Id;
                            StockProcessViewModel.ProductUidId = item.ProductUidId;
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
                                    Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                }
                            }

                            line.StockProcessId = StockProcessViewModel.StockProcessId;
                        }

                        if (item.StockInId != null)
                        {
                            StockAdj Adj_IssQty = new StockAdj();
                            Adj_IssQty.StockAdjId = -Cnt;
                            Adj_IssQty.StockInId = (int)item.StockInId;
                            Adj_IssQty.StockOutId = (int)line.StockId;
                            Adj_IssQty.DivisionId = Header.DivisionId;
                            Adj_IssQty.SiteId = Header.SiteId;
                            Adj_IssQty.AdjustedQty = item.Qty;
                            Adj_IssQty.ObjectState = Model.ObjectState.Added;
                            db.StockAdj.Add(Adj_IssQty);
                        }

                        line.JobOrderHeaderId = item.JobOrderHeaderId;
                        line.ProdOrderLineId = item.ProdOrderLineId;
                        line.StockInId = item.StockInId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.FromProcessId= item.FromProcessId;
                        line.LotNo = item.LotNo;
                        line.Qty = item.Qty;
                        line.UnitId = item.UnitId;
                        line.Sr = Serial++;
                        line.LossQty = item.LossQty;
                        line.NonCountedQty = item.NonCountedQty;
                        line.Rate = item.Rate;
                        line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        line.Amount = DecimalRoundOff.amountToFixed((line.DealQty * line.Rate), Settings.AmountRoundOff);
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.JobOrderLineId = pk;
                        line.ObjectState = Model.ObjectState.Added;
                        //_JobOrderLineService.Create(line);
                        db.JobOrderLine.Add(line);

                        if (line.ProdOrderLineId.HasValue)
                            LineStatus.Add(line.ProdOrderLineId.Value, line.Qty);


                        new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderLineId, ref db, true);

                        BarCodesPendingToUpdate.Add(line);


                        #region BOMPost

                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        if (!string.IsNullOrEmpty(Settings.SqlProcConsumption))
                        {
                            var BomPostList = _JobOrderLineService.GetBomPostingDataForWeaving(line.ProductId, line.Dimension1Id, line.Dimension2Id, line.Dimension3Id, line.Dimension4Id, Header.ProcessId, line.Qty, Header.DocTypeId, Settings.SqlProcConsumption).ToList();

                            foreach (var Bomitem in BomPostList)
                            {
                                JobOrderBom BomPost = new JobOrderBom();
                                BomPost.CreatedBy = User.Identity.Name;
                                BomPost.CreatedDate = DateTime.Now;
                                BomPost.Dimension1Id = Bomitem.Dimension1Id;
                                BomPost.Dimension2Id = Bomitem.Dimension2Id;
                                BomPost.Dimension3Id = Bomitem.Dimension3Id;
                                BomPost.Dimension4Id = Bomitem.Dimension4Id;
                                BomPost.JobOrderHeaderId = line.JobOrderHeaderId;
                                BomPost.JobOrderLineId = line.JobOrderLineId;
                                BomPost.ModifiedBy = User.Identity.Name;
                                BomPost.ModifiedDate = DateTime.Now;
                                BomPost.ProductId = Bomitem.ProductId;
                                BomPost.Qty = Convert.ToDecimal(Bomitem.Qty);
                                BomPost.BOMQty = Convert.ToDecimal(Bomitem.BOMQty);
                                BomPost.ObjectState = Model.ObjectState.Added;
                                db.JobOrderBom.Add(BomPost);
                                //new JobOrderBomService(_unitOfWork).Create(BomPost);
                            }
                        }



                        #endregion


                        if (Settings != null)
                        {
                            new CommonService().ExecuteCustomiseEvents(Settings.Event_OnLineSave, new object[] { _unitOfWork, line.JobOrderHeaderId, line.JobOrderLineId, "A" });
                        }


                        if (Settings.CalculationId.HasValue)
                        {
                            LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobOrderLineId, HeaderTableId = item.JobOrderHeaderId, PersonID = Header.JobWorkerId, DealQty = line.DealQty });
                        }

                        List<CalculationProductViewModel> ChargeRates = new CalculationProductService(_unitOfWork).GetChargeRates(CalculationId, Header.DocTypeId, Header.SiteId, Header.DivisionId,
                            Header.ProcessId, item.SalesTaxGroupPersonId, item.SalesTaxGroupProductId).ToList();
                        if (ChargeRates != null)
                        {
                            LineChargeRates.Add(new LineChargeRates { LineId = line.JobOrderLineId, ChargeRates = ChargeRates });
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
                //new JobOrderHeaderService(_unitOfWork).Update(Header);
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderHeader.Add(Header);

                new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyJobOrderMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    if (Settings.CalculationId.HasValue)
                    {
                        var LineListWithReferences = (from p in LineList
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
                                                          ChargeRates = LineChargeRatesTab.ChargeRates,
                                                      }).ToList();


                        new ChargesCalculationService(_unitOfWork).CalculateCharges(LineListWithReferences, vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobOrderHeaderCharges", "Web.JobOrderLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                        //Saving Charges
                        foreach (var item in LineCharges)
                        {

                            JobOrderLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobOrderLineCharge>(item);
                            PoLineCharge.ObjectState = Model.ObjectState.Added;
                            db.JobOrderLineCharge.Add(PoLineCharge);
                            //new JobOrderLineChargeService(_unitOfWork).Create(PoLineCharge);

                        }


                        //Saving Header charges
                        for (int i = 0; i < HeaderCharges.Count(); i++)
                        {

                            if (!HeaderChargeEdit)
                            {
                                JobOrderHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobOrderHeaderCharge>(HeaderCharges[i]);
                                POHeaderCharge.HeaderTableId = vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId;
                                POHeaderCharge.PersonID = Header.JobWorkerId;
                                POHeaderCharge.ObjectState = Model.ObjectState.Added;
                                db.JobOrderHeaderCharges.Add(POHeaderCharge);
                                //new JobOrderHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                            }
                            else
                            {
                                var footercharge = new JobOrderHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                                footercharge.Rate = HeaderCharges[i].Rate;
                                footercharge.Amount = HeaderCharges[i].Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.JobOrderHeaderCharges.Add(footercharge);
                                //new JobOrderHeaderChargeService(_unitOfWork).Update(footercharge);
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
                    JobOrderDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId), ref db);
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


                if (Settings != null)
                {
                    if (Settings.Event_AfterLineSave != null)
                    {
                        IEnumerable<JobOrderLineViewModel> linelist = new JobOrderLineService(_unitOfWork).GetJobOrderLineListForIndex(Header.JobOrderHeaderId);

                        foreach (var item in linelist)
                        {
                            new CommonService().ExecuteCustomiseEvents(Settings.Event_AfterLineSave, new object[] { _unitOfWork, item.JobOrderHeaderId, item.JobOrderLineId, "A" });
                        }
                    }
                }

                try
                {
                    JobOrderDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderLineViewModel.FirstOrDefault().JobOrderHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderHeaderId,
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
            var p = _JobOrderLineService.GetJobOrderLineListForIndex(id).ToList();

            JobOrderHeader Header = new JobOrderHeaderService(_unitOfWork).Find(id);

            return Json(p, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult _Index(int id, int Status)
        {
            ViewBag.Status = Status;
            ViewBag.JobOrderHeaderId = id;
            var p = _JobOrderLineService.GetJobOrderLineListForIndex(id).ToList();
            return PartialView(p);
        }


        [HttpGet]
        public JsonResult ConsumptionIndex(int id)
        {
            var p = _JobOrderLineService.GetConsumptionLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        private void PrepareViewBag(JobOrderLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            if (vm != null)
            {
                JobOrderHeaderViewModel H = new JobOrderHeaderService(_unitOfWork).GetJobOrderHeader(vm.JobOrderHeaderId);
                ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
            }
        }

        [HttpGet]
        public ActionResult CreateLine(int id, string LineNature)
        {
            return _Create(id, null, LineNature);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, string LineNature)
        {
            return _Create(id, null, LineNature);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, string LineNature)
        {
            return _Create(id, null, LineNature);
        }

        public ActionResult _Create(int Id, DateTime? date, string LineNature) //Id ==>Sale Order Header Id
        {
            JobOrderHeader H = new JobOrderHeaderService(_unitOfWork).Find(Id);
            JobOrderLineViewModel s = new JobOrderLineViewModel();

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            var count = _JobOrderLineService.GetJobOrderLineListForIndex(Id).Count();
            if (count > 0)
            {
                s.NonCountedQty = _JobOrderLineService.GetJobOrderLineListForIndex(Id).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault().NonCountedQty;
                s.LossQty = _JobOrderLineService.GetJobOrderLineListForIndex(Id).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault().LossQty;
                s.DealUnitId = _JobOrderLineService.GetJobOrderLineListForIndex(Id).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault().DealUnitId;
            }
            else
            {
                s.NonCountedQty = settings.NonCountedQty;
                s.LossQty = settings.LossQty;
                s.DealUnitId = settings.DealUnitId;
            }


            if (H.SalesTaxGroupPersonId != null)
                s.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (s.CalculationId == null)
                s.CalculationId = settings.CalculationId;

            s.GodownId = H.GodownId;
            s.AllowRepeatProcess = false;
            s.JobOrderHeaderId = H.JobOrderHeaderId;
            ViewBag.Status = H.Status;
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

            s.LineNature = LineNature;
            return PartialView("_Create", s);

            //if (IsProdBased == true)
            //{
            //    return PartialView("_CreateForProdOrder", s);

            //}
            //else
            //{
            //    return PartialView("_Create", s);
            //}

        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobOrderLineViewModel svm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool BeforeSave = true;
            JobOrderHeader temp = new JobOrderHeaderService(_unitOfWork).Find(svm.JobOrderHeaderId);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (settings != null)
            {
                if (settings.isVisibleProcessLine == true && settings.isMandatoryProcessLine == true && (svm.FromProcessId <= 0 || svm.FromProcessId == null))
                {
                    ModelState.AddModelError("FromProcessId", "The Process field is required");
                }
                if (settings.isVisibleRate == true && settings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
                if (settings.isVisibleProductUID == true && settings.isMandatoryProductUID == true && !svm.ProductUidId.HasValue)
                {
                    ModelState.AddModelError("ProductUidIdName", "The ProductUid field is required");
                }

                if (settings.IsMandatoryStockIn == true)
                {
                    if (svm.StockInId == null)
                    {
                        ModelState.AddModelError("StockInId", "Stock No field is required");
                    }
                }
            }

            #region BeforeSave
            try
            {

                if (svm.JobOrderLineId <= 0)
                    BeforeSave = JobOrderDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobOrderDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderHeaderId, EventModeConstants.Edit), ref db);

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

            //        if (svm.SalesTaxGroupProductId != 0 && svm.SalesTaxGroupProductId != null && svm.SalesTaxGroupPersonId != 0 && svm.SalesTaxGroupPersonId != null && svm.JobOrderSettings.CalculationId != null)
            //        {
            //            IEnumerable<ChargeRateSettings> ChargeRateSettingsList = new CalculationProductService(_unitOfWork).GetChargeRateSettingForValidation((int)svm.JobOrderSettings.CalculationId, temp.DocTypeId, temp.SiteId, temp.DivisionId, temp.ProcessId, (int)svm.SalesTaxGroupPersonId, (int)svm.SalesTaxGroupProductId);

            //            foreach (var item in ChargeRateSettingsList)
            //            {
            //                if (item.ChargeGroupSettingId == null)
            //                {
            //                    ModelState.AddModelError("", "Charge Group Setting is not defined for " + item.ChargeName + ".");
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion

            JobOrderLine s = Mapper.Map<JobOrderLineViewModel, JobOrderLine>(svm);

            ViewBag.Status = temp.Status;


            if (temp.DocDate > s.DueDate && s.DueDate != null)
            {
                ModelState.AddModelError("DueDate", "Duedate should be greater than Docdate");
            }

            if (svm.JobOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.JobOrderLineId <= 0)
                {
                    StockViewModel StockViewModel = new StockViewModel();
                    StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                    //Posting in Stock
                    if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true)
                    {
                        StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockViewModel.DocHeaderId = temp.JobOrderHeaderId;
                        StockViewModel.DocLineId = s.JobOrderLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = null;
                        StockViewModel.PersonId = temp.JobWorkerId;
                        StockViewModel.ProductId = s.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = null;
                        StockViewModel.GodownId = temp.GodownId ?? 0;
                        StockViewModel.ProcessId = s.FromProcessId;
                        StockViewModel.LotNo = s.LotNo;
                        StockViewModel.CostCenterId = temp.CostCenterId;
                        StockViewModel.Qty_Iss = s.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = s.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = s.Specification;
                        StockViewModel.Dimension1Id = s.Dimension1Id;
                        StockViewModel.Dimension2Id = s.Dimension2Id;
                        StockViewModel.Dimension3Id = s.Dimension3Id;
                        StockViewModel.Dimension4Id = s.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.ProductUidId = s.ProductUidId;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.CreatedBy = temp.CreatedBy;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = temp.ModifiedBy;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }

                        s.StockId = StockViewModel.StockId;

                        if (temp.StockHeaderId == null)
                        {
                            temp.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                    }



                    if (settings.isPostedInStockProcess.HasValue && settings.isPostedInStockProcess == true)
                    {
                        if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessViewModel.StockHeaderId = (int)temp.StockHeaderId;
                        }
                        else if (settings.isPostedInStock.HasValue && settings.isPostedInStock == true)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                        {
                            StockProcessViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessViewModel.StockHeaderId = 0;
                        }


                        StockProcessViewModel.DocHeaderId = temp.JobOrderHeaderId;
                        StockProcessViewModel.DocLineId = s.JobOrderLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = null;
                        StockProcessViewModel.PersonId = temp.JobWorkerId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = null;
                        StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = s.LotNo;
                        StockProcessViewModel.CostCenterId = temp.CostCenterId;
                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = s.Qty;
                        StockProcessViewModel.Rate = s.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = s.Specification;
                        StockProcessViewModel.Dimension1Id = s.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = s.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = s.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = s.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.ProductUidId = s.ProductUidId;
                        StockProcessViewModel.CreatedBy = temp.CreatedBy;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            if (svm.ProdOrderLineId != null)
                            {
                                return PartialView("_CreateForProdOrder", svm);
                            }
                            else
                            {
                                return PartialView("_Create", svm);
                            }
                        }

                        s.StockProcessId = StockProcessViewModel.StockProcessId;


                        if (settings.isPostedInStock == false)
                        {
                            if (temp.StockHeaderId == null)
                            {
                                temp.StockHeaderId = StockProcessViewModel.StockHeaderId;
                            }
                        }
                    }

                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)s.StockId;
                        Adj_IssQty.DivisionId = temp.DivisionId;
                        Adj_IssQty.SiteId = temp.SiteId;
                        Adj_IssQty.AdjustedQty = s.Qty;
                        Adj_IssQty.ObjectState = Model.ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                        //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.Sr = _JobOrderLineService.GetMaxSr(s.JobOrderHeaderId);
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;

                    if (s.ProductUidId.HasValue && s.ProductUidId > 0)
                    {
                        ProductUid Uid = (from p in db.ProductUid
                                          where p.ProductUIDId == s.ProductUidId
                                          select p).FirstOrDefault();


                        s.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                        s.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                        s.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                        s.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                        s.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                        s.ProductUidStatus = Uid.Status;
                        s.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                        s.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                        Uid.LastTransactionDocId = s.JobOrderHeaderId;
                        Uid.LastTransactionDocDate = temp.DocDate;
                        Uid.LastTransactionDocNo = temp.DocNo;
                        Uid.LastTransactionDocTypeId = temp.DocTypeId;
                        Uid.LastTransactionLineId = s.JobOrderLineId;
                        Uid.LastTransactionPersonId = temp.JobWorkerId;
                        Uid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Issue);
                        Uid.CurrenctProcessId = temp.ProcessId;
                        Uid.CurrenctGodownId = null;
                        Uid.ModifiedBy = User.Identity.Name;
                        Uid.ModifiedDate = DateTime.Now;
                        Uid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(Uid);
                    }


                    db.JobOrderLine.Add(s);

                    new JobOrderLineStatusService(_unitOfWork).CreateLineStatus(s.JobOrderLineId, ref db, true);

                    if (s.ProdOrderLineId.HasValue)
                        new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyOnJobOrder(s.ProdOrderLineId.Value, s.JobOrderLineId, temp.DocDate, s.Qty, ref db, true);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.JobOrderLineId;
                            item.PersonID = temp.JobWorkerId;
                            item.DealQty = s.DealQty;
                            item.HeaderTableId = temp.JobOrderHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.JobOrderLineCharge.Add(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new JobOrderHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.JobOrderHeaderCharges.Add(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.JobOrderHeaderId;
                                item.PersonID = temp.JobWorkerId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.JobOrderHeaderCharges.Add(item);
                            }
                        }


                    //JobOrderHeader header = new JobOrderHeaderService(_unitOfWork).Find(s.JobOrderHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;

                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderHeader.Add(temp);


                    #region BOMPost

                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    if (!string.IsNullOrEmpty(svm.JobOrderSettings.SqlProcConsumption))
                    {
                        var BomPostList = _JobOrderLineService.GetBomPostingDataForWeaving(s.ProductId, s.Dimension1Id, s.Dimension2Id, s.Dimension3Id, s.Dimension4Id, temp.ProcessId, s.Qty, temp.DocTypeId, svm.JobOrderSettings.SqlProcConsumption).ToList();

                        foreach (var item in BomPostList)
                        {
                            JobOrderBom BomPost = new JobOrderBom();
                            BomPost.CreatedBy = User.Identity.Name;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.Dimension1Id = item.Dimension1Id;
                            BomPost.Dimension2Id = item.Dimension2Id;
                            BomPost.Dimension3Id = item.Dimension3Id;
                            BomPost.Dimension4Id = item.Dimension4Id;
                            BomPost.JobOrderHeaderId = s.JobOrderHeaderId;
                            BomPost.JobOrderLineId = s.JobOrderLineId;
                            BomPost.ModifiedBy = User.Identity.Name;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = item.ProductId;
                            BomPost.Qty = Convert.ToDecimal(item.Qty);
                            BomPost.ObjectState = Model.ObjectState.Added;
                            db.JobOrderBom.Add(BomPost);
                            //new JobOrderBomService(_unitOfWork).Create(BomPost);
                        }
                    }

                    //try
                    //{
                    //    _unitOfWork.Save();
                    //}

                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    ModelState.AddModelError("", message);
                    //    PrepareViewBag(svm);
                    //    return PartialView("_Create", svm);

                    //}

                    #endregion

                    try
                    {
                        JobOrderDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobOrderHeaderId, s.JobOrderLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    if (settings != null)
                    {
                        new CommonService().ExecuteCustomiseEvents(settings.Event_OnLineSave, new object[] { _unitOfWork, svm.JobOrderHeaderId, s.JobOrderLineId, "A" });
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

                        if (svm.ProdOrderLineId != null)
                        {
                            return PartialView("_CreateForProdOrder", svm);
                        }
                        else
                        {
                            return PartialView("_Create", svm);
                        }

                    }

                    if (settings != null)
                    {
                        new CommonService().ExecuteCustomiseEvents(settings.Event_AfterLineSave, new object[] { _unitOfWork, svm.JobOrderHeaderId, s.JobOrderLineId, "A" });
                    }

                    try
                    {
                        JobOrderDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobOrderHeaderId, s.JobOrderLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderHeaderId,
                        DocLineId = s.JobOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.JobOrderHeaderId, LineNature = svm.LineNature });

                }


                else
                {
                    JobOrderLine templine = (from p in db.JobOrderLine
                                             where p.JobOrderLineId == s.JobOrderLineId
                                             select p).FirstOrDefault();

                    JobOrderLine ExTempLine = new JobOrderLine();
                    ExTempLine = Mapper.Map<JobOrderLine>(templine);

                    if (templine.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockViewModel.StockId = templine.StockId ?? 0;
                        StockViewModel.DocHeaderId = templine.JobOrderHeaderId;
                        StockViewModel.DocLineId = templine.JobOrderLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = temp.ProcessId;
                        StockViewModel.PersonId = temp.JobWorkerId;
                        StockViewModel.ProductId = s.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = temp.GodownId;
                        StockViewModel.GodownId = temp.GodownId ?? 0;
                        StockViewModel.ProcessId = templine.FromProcessId;
                        StockViewModel.LotNo = templine.LotNo;
                        StockViewModel.CostCenterId = temp.CostCenterId;
                        StockViewModel.Qty_Iss = s.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = templine.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = templine.Specification;
                        StockViewModel.Dimension1Id = templine.Dimension1Id;
                        StockViewModel.Dimension2Id = templine.Dimension2Id;
                        StockViewModel.Dimension3Id = templine.Dimension3Id;
                        StockViewModel.Dimension4Id = templine.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.ProductUidId = s.ProductUidId;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.CreatedBy = templine.CreatedBy;
                        StockViewModel.CreatedDate = templine.CreatedDate;
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




                    if (templine.StockProcessId != null)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                        StockProcessViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                        StockProcessViewModel.StockProcessId = templine.StockProcessId ?? 0;
                        StockProcessViewModel.DocHeaderId = templine.JobOrderHeaderId;
                        StockProcessViewModel.DocLineId = templine.JobOrderLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.JobWorkerId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = templine.LotNo;
                        StockProcessViewModel.CostCenterId = temp.CostCenterId;
                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = s.Qty;
                        StockProcessViewModel.Rate = templine.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = templine.Specification;
                        StockProcessViewModel.Dimension1Id = templine.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = templine.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = templine.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = templine.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.ProductUidId = s.ProductUidId;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.CreatedBy = templine.CreatedBy;
                        StockProcessViewModel.CreatedDate = templine.CreatedDate;
                        StockProcessViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            if (svm.ProdOrderLineId != null)
                            {
                                return PartialView("_CreateForProdOrder", svm);
                            }
                            else
                            {
                                return PartialView("_Create", svm);
                            }
                        }
                    }

                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == templine.StockId
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        Adj.ObjectState = Model.ObjectState.Deleted;
                        db.StockAdj.Remove(Adj);
                        //new StockAdjService(_unitOfWork).Delete(Adj);
                    }

                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)templine.StockId;
                        Adj_IssQty.DivisionId = temp.DivisionId;
                        Adj_IssQty.SiteId = temp.SiteId;
                        Adj_IssQty.AdjustedQty = svm.Qty;
                        Adj_IssQty.ObjectState = Model.ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                        //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }


                    if (!string.IsNullOrEmpty(settings.SqlProcGenProductUID))
                    {

                        if (templine.ProductUidHeaderId != null)
                        {
                            ProductUidHeader ProdUidHeader = (from p in db.ProductUidHeader
                                                              where p.ProductUidHeaderId == templine.ProductUidHeaderId
                                                              select p).FirstOrDefault();

                            ProdUidHeader.ProductId = s.ProductId;
                            ProdUidHeader.Dimension1Id = s.Dimension1Id;
                            ProdUidHeader.Dimension2Id = s.Dimension2Id;
                            ProdUidHeader.Dimension3Id = s.Dimension3Id;
                            ProdUidHeader.Dimension4Id = s.Dimension4Id;
                            ProdUidHeader.GenDocId = s.JobOrderHeaderId;
                            ProdUidHeader.GenDocNo = temp.DocNo;
                            ProdUidHeader.GenDocTypeId = temp.DocTypeId;
                            ProdUidHeader.GenDocDate = temp.DocDate;
                            ProdUidHeader.GenPersonId = temp.JobWorkerId;
                            ProdUidHeader.ModifiedBy = User.Identity.Name;
                            ProdUidHeader.ModifiedDate = DateTime.Now;
                            ProdUidHeader.ObjectState = Model.ObjectState.Modified;
                            db.ProductUidHeader.Add(ProdUidHeader);


                            if (svm.Qty > templine.Qty)
                            {
                                List<string> uids = _JobOrderLineService.GetProcGenProductUids(temp.DocTypeId, svm.Qty - templine.Qty, temp.DivisionId, temp.SiteId);
                                int count = 0;
                                foreach (string item in uids)
                                {
                                    ProductUid ProdUid = new ProductUid();

                                    ProdUid.ProductUidName = item;
                                    ProdUid.ProductId = s.ProductId;
                                    ProdUid.IsActive = true;
                                    ProdUid.CreatedBy = User.Identity.Name;
                                    ProdUid.CreatedDate = DateTime.Now;
                                    ProdUid.ModifiedBy = User.Identity.Name;
                                    ProdUid.ModifiedDate = DateTime.Now;
                                    ProdUid.GenLineId = s.JobOrderLineId;
                                    ProdUid.GenDocId = s.JobOrderHeaderId;
                                    ProdUid.GenDocNo = temp.DocNo;
                                    ProdUid.GenDocTypeId = temp.DocTypeId;
                                    ProdUid.GenDocDate = temp.DocDate;
                                    ProdUid.GenPersonId = temp.JobWorkerId;
                                    ProdUid.Dimension1Id = s.Dimension1Id;
                                    ProdUid.Dimension2Id = s.Dimension2Id;
                                    ProdUid.Dimension3Id = s.Dimension3Id;
                                    ProdUid.Dimension4Id = s.Dimension4Id;
                                    ProdUid.CurrenctProcessId = null;
                                    ProdUid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Issue);
                                    ProdUid.LastTransactionDocId = s.JobOrderHeaderId;
                                    ProdUid.LastTransactionDocNo = temp.DocNo;
                                    ProdUid.LastTransactionDocTypeId = temp.DocTypeId;
                                    ProdUid.LastTransactionDocDate = temp.DocDate;
                                    ProdUid.LastTransactionPersonId = temp.JobWorkerId;
                                    ProdUid.LastTransactionLineId = s.JobOrderLineId;
                                    ProdUid.ProductUIDId = count;
                                    ProdUid.ObjectState = Model.ObjectState.Added;
                                    db.ProductUid.Add(ProdUid);

                                    count++;
                                }
                            }
                            else if (svm.Qty < templine.Qty)
                            {
                                var ProductUidToDelete = (from L in db.ProductUid
                                                          where L.ProductUidHeaderId == ProdUidHeader.ProductUidHeaderId
                                                          select L).Take((int)(templine.Qty - svm.Qty));

                                foreach (var item in ProductUidToDelete)
                                {
                                    item.ObjectState = Model.ObjectState.Deleted;
                                    db.ProductUid.Remove(item);
                                }
                            }
                        }
                    }


                    templine.SalesTaxGroupProductId = svm.SalesTaxGroupProductId;
                    templine.DueDate = s.DueDate;
                    templine.ProductId = s.ProductId;
                    templine.ProductUidId = s.ProductUidId;
                    templine.ProdOrderLineId = s.ProdOrderLineId;
                    templine.DueDate = s.DueDate;
                    templine.LotNo = s.LotNo;
                    templine.FromProcessId = s.FromProcessId;
                    templine.UnitId = s.UnitId;
                    templine.DealUnitId = s.DealUnitId;
                    templine.DealQty = s.DealQty;
                    templine.NonCountedQty = s.NonCountedQty;
                    templine.LossQty = s.LossQty;
                    templine.Rate = s.Rate;
                    templine.DiscountPer = s.DiscountPer;
                    templine.DiscountAmount = s.DiscountAmount;
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
                    templine.StockInId = s.StockInId;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderLine.Add(templine);

                    int Status = 0;
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        Status = temp.Status;
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }


                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderHeader.Add(temp);

                    if (templine.ProdOrderLineId.HasValue)
                        new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyOnJobOrder(templine.ProdOrderLineId.Value, templine.JobOrderLineId, temp.DocDate, templine.Qty, ref db, true);

                    var Boms = (from p in db.JobOrderBom
                                where p.JobOrderLineId == templine.JobOrderLineId
                                select p).ToList();

                    foreach (var item in Boms)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobOrderBom.Remove(item);
                    }


                    #region BOMPost

                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    if (!string.IsNullOrEmpty(svm.JobOrderSettings.SqlProcConsumption))
                    {
                        var BomPostList = _JobOrderLineService.GetBomPostingDataForWeaving(s.ProductId, s.Dimension1Id, s.Dimension2Id, s.Dimension3Id, s.Dimension4Id, temp.ProcessId, s.Qty, temp.DocTypeId, svm.JobOrderSettings.SqlProcConsumption).ToList();

                        foreach (var item in BomPostList)
                        {
                            JobOrderBom BomPost = new JobOrderBom();
                            BomPost.CreatedBy = User.Identity.Name;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.Dimension1Id = item.Dimension1Id;
                            BomPost.Dimension2Id = item.Dimension2Id;
                            BomPost.Dimension3Id = item.Dimension3Id;
                            BomPost.Dimension4Id = item.Dimension4Id;
                            BomPost.JobOrderHeaderId = s.JobOrderHeaderId;
                            BomPost.JobOrderLineId = s.JobOrderLineId;
                            BomPost.ModifiedBy = User.Identity.Name;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = item.ProductId;
                            BomPost.Qty = Convert.ToDecimal(item.Qty);
                            BomPost.ObjectState = Model.ObjectState.Added;
                            db.JobOrderBom.Add(BomPost);
                            //new JobOrderBomService(_unitOfWork).Create(BomPost);
                        }
                    }

                    #endregion



                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExTempLine,
                        Obj = templine
                    });

                    if (svm.linecharges != null)
                    {
                        var ProductChargeList = (from p in db.JobOrderLineCharge
                                                 where p.LineTableId == templine.JobOrderLineId
                                                 select p).ToList();

                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = (ProductChargeList.Where(m => m.Id == item.Id)).FirstOrDefault();

                            var ExProdcharge = Mapper.Map<JobOrderLineCharge>(productcharge);
                            productcharge.Rate = item.Rate ?? 0;
                            productcharge.Amount = item.Amount ?? 0;
                            productcharge.DealQty = templine.DealQty;
                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExProdcharge,
                                Obj = productcharge
                            });
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.JobOrderLineCharge.Add(productcharge);
                        }
                    }

                    if (svm.footercharges != null)
                    {
                        var footerChargerecords = (from p in db.JobOrderHeaderCharges
                                                   where p.HeaderTableId == temp.JobOrderHeaderId
                                                   select p).ToList();

                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = footerChargerecords.Where(m => m.Id == item.Id).FirstOrDefault();
                            var Exfootercharge = Mapper.Map<JobOrderHeaderCharge>(footercharge);
                            footercharge.Rate = item.Rate ?? 0;
                            footercharge.Amount = item.Amount ?? 0;
                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = Exfootercharge,
                                Obj = footercharge,
                            });
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobOrderHeaderCharges.Add(footercharge);
                        }
                    }

                    if (settings != null)
                    {
                        new CommonService().ExecuteCustomiseEvents(settings.Event_OnLineSave, new object[] { _unitOfWork, svm.JobOrderHeaderId, s.JobOrderLineId, "E" });
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobOrderDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobOrderHeaderId, templine.JobOrderLineId, EventModeConstants.Edit), ref db);
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
                        if (svm.ProdOrderLineId != null)
                        {
                            return PartialView("_CreateForProdOrder", svm);
                        }
                        else
                        {
                            return PartialView("_Create", svm);
                        }

                    }

                    try
                    {
                        JobOrderDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobOrderHeaderId, templine.JobOrderLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    if (settings != null)
                    {
                        new CommonService().ExecuteCustomiseEvents(settings.Event_AfterLineSave, new object[] { _unitOfWork, svm.JobOrderHeaderId, s.JobOrderLineId, "E" });
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = templine.JobOrderHeaderId,
                        DocLineId = templine.JobOrderLineId,
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
            if (svm.ProdOrderLineId != null)
            {
                return PartialView("_CreateForProdOrder", svm);
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
            JobOrderLineViewModel temp = _JobOrderLineService.GetJobOrderLine(id);

            JobOrderHeader H = new JobOrderHeaderService(_unitOfWork).Find(temp.JobOrderHeaderId);

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (H.SalesTaxGroupPersonId != null)
                temp.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (temp.CalculationId == null)
                temp.CalculationId = settings.CalculationId;

            //ViewBag.DocNo = H.DocNo;
            temp.GodownId = H.GodownId;
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

            //if (string.IsNullOrEmpty(temp.LockReason) || UserRoles.Contains("SysAdmin"))
            //    ViewBag.LineMode = "Edit";
            //else
            //    TempData["CSEXCL"] += temp.LockReason;

            //if (temp.ProdOrderLineId != null)
            //{
            //    return PartialView("_CreateForProdOrder", temp);

            //}
            //else
            //{
            //    return PartialView("_Create", temp);
            //}

            if (temp.ProdOrderLineId != null)
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
            JobOrderLineViewModel temp = _JobOrderLineService.GetJobOrderLine(id);

            JobOrderHeader H = new JobOrderHeaderService(_unitOfWork).Find(temp.JobOrderHeaderId);

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            temp.GodownId = H.GodownId;
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);
            //ViewBag.LineMode = "Delete";

            if (H.SalesTaxGroupPersonId != null)
                temp.CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(H.DocTypeId, (int)H.SalesTaxGroupPersonId, H.SiteId, H.DivisionId);

            if (temp.CalculationId == null)
                temp.CalculationId = settings.CalculationId;

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

            //if (temp.ProdOrderLineId != null)
            //{
            //    return PartialView("_CreateForProdOrder", temp);
            //}
            //else
            //{
            //    return PartialView("_Create", temp);
            //}

            if (temp.ProdOrderLineId != null)
                temp.LineNature = LineNatureConstants.ForOrder;
            else
                temp.LineNature = LineNatureConstants.Direct;

            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobOrderLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobOrderHeaderId, vm.JobOrderLineId), ref db);
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

                int? StockId = 0;
                int? StockProcessId = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                JobOrderLine JobOrderLine = (from p in db.JobOrderLine
                                             where p.JobOrderLineId == vm.JobOrderLineId
                                             select p).FirstOrDefault();
                JobOrderHeader header = (from p in db.JobOrderHeader
                                         where p.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId
                                         select p).FirstOrDefault();

                JobOrderLineStatus LineStatus = (from p in db.JobOrderLineStatus
                                                 where p.JobOrderLineId == JobOrderLine.JobOrderLineId
                                                 select p).FirstOrDefault();

                StockId = JobOrderLine.StockId;
                StockProcessId = JobOrderLine.StockProcessId;

                LogList.Add(new LogTypeViewModel
                {
                    Obj = Mapper.Map<JobOrderLine>(JobOrderLine),
                });

                LineStatus.ObjectState = Model.ObjectState.Deleted;
                db.JobOrderLineStatus.Remove(LineStatus);
                //new JobOrderLineStatusService(_unitOfWork).Delete(JobOrderLine.JobOrderLineId);

                if (JobOrderLine.ProdOrderLineId.HasValue)
                    new ProdOrderLineStatusService(_unitOfWork).UpdateProdQtyOnJobOrder(JobOrderLine.ProdOrderLineId.Value, JobOrderLine.JobOrderLineId, header.DocDate, 0, ref db, true);




                if ((JobOrderLine.ProductUidHeaderId.HasValue) && (JobOrderLine.ProductUidHeaderId.Value > 0))
                {

                    //var ProductUid = new ProductUidService(_unitOfWork).FindForJobOrderLine(JobOrderLine.ProductUidHeaderId.Value);

                    var ProductUid = (from p in db.ProductUid
                                      where p.ProductUidHeaderId == JobOrderLine.ProductUidHeaderId
                                      select p).ToList();

                    foreach (var item in ProductUid)
                    {
                        if (item.LastTransactionDocId == item.GenDocId && item.LastTransactionDocTypeId == item.GenDocTypeId)
                        {
                            item.ObjectState = Model.ObjectState.Deleted;
                            db.ProductUid.Remove(item);
                            //new ProductUidService(_unitOfWork).Delete(item);
                        }
                        else
                        {
                            EventException = true;
                            TempData["CSEXCL"] = "Record Cannot be deleted as it is in use by other documents";
                            break;
                        }
                    }
                }
                else
                {

                    if (JobOrderLine.ProductUidId.HasValue)
                    {
                        ProductUid ProductUid = (from p in db.ProductUid
                                                 where p.ProductUIDId == JobOrderLine.ProductUidId
                                                 select p).FirstOrDefault();

                        if (header.DocNo != ProductUid.LastTransactionDocNo || header.DocTypeId != ProductUid.LastTransactionDocTypeId)
                        {
                            ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                            PrepareViewBag(vm);
                            ViewBag.LineMode = "Delete";
                            return PartialView("_Create", vm);
                        }

                        ProductUid.LastTransactionDocDate = JobOrderLine.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = JobOrderLine.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = JobOrderLine.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = JobOrderLine.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = JobOrderLine.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = JobOrderLine.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = JobOrderLine.ProductUidCurrentProcessId;
                        ProductUid.Status = JobOrderLine.ProductUidStatus;
                        if (ProductUid.ProcessesDone != null)
                        {
                            ProductUid.ProcessesDone = ProductUid.ProcessesDone.Replace("|" + new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId.ToString() + "|", "");
                        }

                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(ProductUid);

                        //new ProductUidService(_unitOfWork).Update(ProductUid);
                    }
                }


                //_JobOrderLineService.Delete(JobOrderLine);
                JobOrderLine.ObjectState = Model.ObjectState.Deleted;
                db.JobOrderLine.Remove(JobOrderLine);


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

                if (StockProcessId != null)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB((int)StockProcessId, ref db, true);
                }


                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    db.JobOrderHeader.Add(header);
                }

                var chargeslist = (from p in db.JobOrderLineCharge
                                   where p.LineTableId == vm.JobOrderLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobOrderLineCharge.Remove(item);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = (from p in db.JobOrderHeaderCharges
                                      where p.Id == item.Id
                                      select p).FirstOrDefault();

                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.JobOrderHeaderCharges.Add(footer);
                    }


                var Boms = (from p in db.JobOrderBom
                            where p.JobOrderLineId == vm.JobOrderLineId
                            select p).ToList();

                foreach (var item in Boms)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobOrderBom.Remove(item);
                }



                var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (settings != null)
                {
                    new CommonService().ExecuteCustomiseEvents(settings.Event_OnLineDelete, new object[] { _unitOfWork, header.JobOrderHeaderId, JobOrderLine.JobOrderLineId });
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobOrderDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobOrderLine.JobOrderHeaderId, JobOrderLine.JobOrderLineId), ref db);
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
                    JobOrderDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobOrderLine.JobOrderHeaderId, JobOrderLine.JobOrderLineId), ref db);
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
                    DocId = header.JobOrderHeaderId,
                    DocLineId = JobOrderLine.JobOrderLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                  
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus=header.Status,
                }));
            }

            return Json(new { success = true });

        }



        public JsonResult GetProductDetailJson(int ProductId, int JobOrderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            //List<Product> ProductJson = new List<Product>();

            //ProductJson.Add(new Product()
            //{
            //    ProductId = product.ProductId,
            //    StandardCost = product.StandardCost,
            //    UnitId = product.UnitId
            //});            

            var DealUnit = _JobOrderLineService.GetJobOrderLineListForIndex(JobOrderId).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault();

            //Decimal Rate = _JobOrderLineService.GetJobRate(JobOrderId, ProductId);

            Decimal Rate = 0;
            Decimal Discount = 0;
            Decimal Incentive = 0;
            Decimal Loss = 0;

            IEnumerable<JobRate> RateList = _JobOrderLineService.GetJobRate(JobOrderId, ProductId);

            if (RateList != null)
            {
                Rate = RateList.FirstOrDefault().Rate ?? 0;
                Discount = RateList.FirstOrDefault().DiscountRate ?? 0;
                Incentive = RateList.FirstOrDefault().IncentiveRate ?? 0;
                Loss = RateList.FirstOrDefault().LossRate ?? 0;
            }



            var Record = new JobOrderHeaderService(_unitOfWork).Find(JobOrderId);

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Record.DocTypeId, Record.DivisionId, Record.SiteId);

            //var DlUnit = new UnitService(_unitOfWork).Find(!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : ((DealUnit == null) ? (Settings.DealUnitId == null ? product.UnitId : Settings.DealUnitId) : DealUnit.DealUnitId));

            Unit DlUnit = new Unit();
            if (Settings.isVisibleDealUnit == false)
            {
                DlUnit = new UnitService(_unitOfWork).Find((!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : product.UnitId));
            }
            else
            {
                DlUnit = new UnitService(_unitOfWork).Find(!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : ((DealUnit == null) ? (Settings.DealUnitId == null ? product.UnitId : Settings.DealUnitId) : DealUnit.DealUnitId));
            }


            return Json(new { ProductId = product.ProductId, 
                StandardCost = Rate, 
                Discount = Discount, 
                Incentive = Incentive, 
                Loss = Loss, 
                UnitId = (!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : product.UnitId), 
                //DealUnitId = !string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : ((DealUnit == null) ? (Settings.DealUnitId == null ? product.UnitId : Settings.DealUnitId) : DealUnit.DealUnitId), 
                DealUnitId = DlUnit.UnitId, 
                DealUnitDecimalPlaces = DlUnit.DecimalPlaces,
                Specification = product.ProductSpecification,
                SalesTaxGroupProductId = product.SalesTaxGroupProductId,
                SalesTaxGroupProductName = product.SalesTaxGroupProductName
            });
        }
        public JsonResult getunitconversiondetailjson(int prodid, string UnitId, string DealUnitId, int JobOrderId)
        {


            var Header = new JobOrderHeaderService(_unitOfWork).Find(JobOrderId);

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

        public JsonResult GetPendingProdOrders(int ProductId)
        {
            return Json(new ProdOrderHeaderService(_unitOfWork).GetPendingProdOrders(ProductId).ToList());
        }

        public JsonResult GetProdOrderDetail(int ProdOrderLineId, int JobOrderHeaderId)
        {
            var temp = new ProdOrderLineService(_unitOfWork).GetProdOrderDetailBalance(ProdOrderLineId);
            var product = new ProductService(_unitOfWork).Find(temp.ProductId);

            var DealUnitId = _JobOrderLineService.GetJobOrderLineListForIndex(JobOrderHeaderId).OrderByDescending(m => m.JobOrderLineId).FirstOrDefault();

            var Record = new JobOrderHeaderService(_unitOfWork).Find(JobOrderHeaderId);

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Record.DocTypeId, Record.DivisionId, Record.SiteId);

            //var DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? (Settings.DealUnitId == null ? temp.UnitId : Settings.DealUnitId) : DealUnitId.DealUnitId);
            Unit DlUnit = new Unit();
            if (Settings.isVisibleDealUnit == false)
            {
                DlUnit = new UnitService(_unitOfWork).Find((!string.IsNullOrEmpty(Settings.JobUnitId) ? Settings.JobUnitId : product.UnitId));
            }
            else
            {
                DlUnit = new UnitService(_unitOfWork).Find((DealUnitId == null) ? (Settings.DealUnitId == null ? temp.UnitId : Settings.DealUnitId) : DealUnitId.DealUnitId);
            }



            temp.DealunitDecimalPlaces = DlUnit.DecimalPlaces;
            temp.DealUnitId = DlUnit.UnitId;

            return Json(temp);
        }

        public JsonResult GetProdOrderForBarCode(int ProdUId, int JobOrderHeaderId)
        {
            var temp = new ProdOrderLineService(_unitOfWork).GetProdOrderForProdUid(ProdUId);
            var detail = GetProdOrderDetail(temp.ProdOrderLineId, JobOrderHeaderId);

            return Json(new { ProdOrder = temp, Detail = detail });
        }


        public JsonResult GetProdOrders(int id, string term, int Limit)//Order Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobOrderLineService.GetPendingProdOrdersWithPatternMatch(id, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingProdOrdersHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var temp = _JobOrderLineService.GetPendingProdOrderHelpList(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            //var count = _JobOrderLineService.GetPendingProdOrderHelpList(filter, searchTerm).Count();
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
        //    return Json(_JobOrderLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        //}

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

        public ActionResult IsProcessDone_ByStockIn(int  StockInId, int ProcessId)
        {
            Stock StockId = new StockService(_unitOfWork).Find(StockInId);
            ProductUid ProductUid; 
            string ProductUidName = "";
            if (StockId.ProductUidId == null)
            {
                ProductUidName = StockId.LotNo;
                ProductUid = new ProductUidService(_unitOfWork).Find(ProductUidName);
            }
            else
            {
                ProductUid = new ProductUidService(_unitOfWork).Find((int)StockId.ProductUidId);
            }
            

            int ProductUidId = 0;
            if (ProductUid != null)
            {
                ProductUidId = ProductUid.ProductUIDId;
            }

            return Json(new ProductUidService(_unitOfWork).IsProcessDone(ProductUidId, ProcessId));
        }

        public JsonResult GetProductPrevProcess(int ProductId, int? GodownId, int DocTypeId)
        {
            ProductPrevProcess ProductPrevProcess = new ProductService(_unitOfWork).FGetProductPrevProcess(ProductId, (GodownId ?? 0), DocTypeId);
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
            var Query = _JobOrderLineService.GetCustomProductGroups(filter, searchTerm);
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

        public ActionResult GetProdOrderForProduct(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _JobOrderLineService.GetProdOrderHelpListForProduct(filter, searchTerm);
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

        public JsonResult SetSingleProdOrderLine(int Ids)
        {
            ComboBoxResult ProdOrderJson = new ComboBoxResult();

            var ProdOrderLine = from L in db.ProdOrderLine
                                  join H in db.ProdOrderHeader on L.ProdOrderHeaderId equals H.ProdOrderHeaderId into ProdOrderHeaderTable
                                  from ProdOrderHeaderTab in ProdOrderHeaderTable.DefaultIfEmpty()
                                  where L.ProdOrderLineId == Ids
                                  select new
                                  {
                                      ProdOrderLineId = L.ProdOrderLineId,
                                      ProdOrderNo = L.Product.ProductName
                                  };

            ProdOrderJson.id = ProdOrderLine.FirstOrDefault().ToString();
            ProdOrderJson.text = ProdOrderLine.FirstOrDefault().ProdOrderNo;

            return Json(ProdOrderJson);
        }



        public ActionResult GetFirstProdOrderForProductUid(int JobOrderHeaderId, int ProductUidId, string term)
        {
            var Query = _JobOrderLineService.GetPendingProdOrderForProductUid(JobOrderHeaderId, ProductUidId, "");
            var temp = Query.ToList();

            var count = Query.Count();


            if (count >= 1)
            {
                return Json(temp.FirstOrDefault());
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetProdOrderForProductUid(string searchTerm, int pageSize, int pageNum, int JobOrderHeaderId, int ProductUidId, string term)
        {
            var Query = _JobOrderLineService.GetPendingProdOrderForProductUid(JobOrderHeaderId, ProductUidId, searchTerm);
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

        public ActionResult GetProductUidHelpList(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceHeaderId
        {
            List<ComboBoxResult> ProductUidJson = _JobOrderLineService.FGetProductUidHelpList(filter, searchTerm).ToList();

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
            var Query = _JobOrderLineService.GetCustomProducts(filter, searchTerm);
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

        public ActionResult GetStockInForProduct(string searchTerm, int pageSize, int pageNum, int filter, int? ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id)//DocTypeId
        {
            var Query = _JobOrderLineService.GetPendingStockInForIssue(filter, ProductId, Dimension1Id, Dimension2Id, Dimension3Id, Dimension4Id, searchTerm);
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

        public ActionResult GetStockInHeader(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _JobOrderLineService.GetPendingStockInHeaderForIssue(filter, searchTerm);
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

        public JsonResult GetStockInDetailJson(int StockInId)
        {
            var temp = (from p in db.ViewStockInBalance
                        join S in db.Stock on p.StockInId  equals S.StockId into StockTable from StockTab in StockTable.DefaultIfEmpty()
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where p.StockInId == StockInId
                        select new
                        {
                            ProductUidId = StockTab.ProductUidId,
                            ProductUidName = StockTab.ProductUid.ProductUidName,
                            ProductId = p.ProductId,
                            ProductName = ProductTab.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = p.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Id = p.Dimension3Id,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Id = p.Dimension4Id,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            BalanceQty = p.BalanceQty,
                            LotNo = p.LotNo,
                            FromProcessId = StockTab.ProcessId,
                            FromProcessName = StockTab.Process.ProcessName
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
