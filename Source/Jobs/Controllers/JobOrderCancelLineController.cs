using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using AutoMapper;
using Model.ViewModels;
using Model.ViewModel;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using JobOrderCancelDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;
        IJobOrderCancelLineService _JobOrderCancelLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public JobOrderCancelLineController(IJobOrderCancelLineService JobOrderCancelLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderCancelLineService = JobOrderCancelLineService;
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
            var p = _JobOrderCancelLineService.GetJobOrderCancelLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            JobOrderCancelFilterViewModel vm = new JobOrderCancelFilterViewModel();
            vm.JobOrderCancelHeaderId = id;
            JobOrderCancelHeader Header = new JobOrderCancelHeaderService(_unitOfWork).Find(id);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            vm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.JobWorkerId = sid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderCancelFilterViewModel vm)
        {
            List<JobOrderCancelLineViewModel> temp = _JobOrderCancelLineService.GetJobOrderLineForMultiSelect(vm).ToList();
            JobOrderCancelMasterDetailModel svm = new JobOrderCancelMasterDetailModel();
            svm.JobOrderCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderCancelMasterDetailModel vm)
        {
            int Cnt = 0;
            int Pk = 0;
            int Serial = _JobOrderCancelLineService.GetMaxSr(vm.JobOrderCancelViewModels.FirstOrDefault().JobOrderCancelHeaderId);
            List<JobOrderCancelLineViewModel> BarCodeBased = new List<JobOrderCancelLineViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderCancelDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderCancelViewModels.FirstOrDefault().JobOrderCancelHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            int[] LineIds = vm.JobOrderCancelViewModels.Select(m => m.JobOrderLineId).ToArray();

            var BalCostCenterRecords = (from L in db.JobOrderLine
                                        join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                                        from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                                        where LineIds.Contains(L.JobOrderLineId)
                                        select new { CostCenterId = JobOrderHeaderTab.CostCenterId, Rate = L.Rate, JobOrderLineId = L.JobOrderLineId }).ToList();

            var BalanceQtyRecords = (from p in db.ViewJobOrderBalance
                                     where LineIds.Contains(p.JobOrderLineId)
                                     select new { BalQty = p.BalanceQty, JobOrderLineId = p.JobOrderLineId }).ToList();

            var JobOrderLineRecords = (from p in db.JobOrderLine
                                       where LineIds.Contains(p.JobOrderLineId)
                                       select p).ToList();

            var HeaderIds = JobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                         where HeaderIds.Contains(p.JobOrderHeaderId)
                                         select p).ToList();
            var ProductUids = JobOrderLineRecords.Select(m => m.ProductUidId).ToArray();

            var ProductUidsRecords = (from p in db.ProductUid
                                      where ProductUids.Contains(p.ProductUIDId)
                                      select p).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobOrderCancelHeader Header = new JobOrderCancelHeaderService(_unitOfWork).Find(vm.JobOrderCancelViewModels.FirstOrDefault().JobOrderCancelHeaderId);
                JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                foreach (var item in vm.JobOrderCancelViewModels)
                {

                    //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(item.JobOrderLineId);
                    //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    JobOrderLine JobOrderLine = JobOrderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                    JobOrderHeader JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();

                    //var temp = (from L in db.JobOrderLine
                    //            join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                    //            from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                    //            where L.JobOrderLineId == item.JobOrderLineId
                    //            select new { CostCenterId = JobOrderHeaderTab.CostCenterId, Rate = L.Rate }).FirstOrDefault();

                    var temp = (from L in BalCostCenterRecords
                                where L.JobOrderLineId == item.JobOrderLineId
                                select L).FirstOrDefault();

                    //decimal balqty = (from p in db.ViewJobOrderBalance
                    //                  where p.JobOrderLineId == item.JobOrderLineId
                    //                  select p.BalanceQty).FirstOrDefault();


                    decimal balqty = (from L in BalanceQtyRecords
                                      where L.JobOrderLineId == item.JobOrderLineId
                                      select L.BalQty).FirstOrDefault();

                    if (JobOrderLine.ProductUidHeaderId != null && item.Qty > 0)
                    {
                        item.BalanceQty = balqty;
                        BarCodeBased.Add(item);
                    }



                    if (item.Qty > 0 && item.Qty <= balqty && JobOrderLine.ProductUidHeaderId == null)
                    {
                        JobOrderCancelLine line = new JobOrderCancelLine();

                        line.JobOrderCancelHeaderId = item.JobOrderCancelHeaderId;
                        line.JobOrderLineId = item.JobOrderLineId;
                        line.Qty = item.Qty;
                        line.ProductUidId = JobOrderLine.ProductUidId;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.JobOrderCancelLineId = Pk++;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.Remark;
                        LineStatus.Add(line.JobOrderLineId, line.Qty);


                        if (line.ProductUidId.HasValue)
                        {

                            //ProductUid Uid = new ProductUidService(_unitOfWork).Find(line.ProductUidId.Value);
                            ProductUid Uid = ProductUidsRecords.Where(m => m.ProductUIDId == line.ProductUidId).FirstOrDefault();

                            line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            line.ProductUidStatus = Uid.Status;
                            line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;

                            if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId==17)
                            {


                                Uid.LastTransactionDocId = Header.JobOrderCancelHeaderId;
                                Uid.LastTransactionDocDate = Header.DocDate;
                                Uid.LastTransactionDocNo = Header.DocNo;
                                Uid.LastTransactionDocTypeId = Header.DocTypeId;
                                Uid.LastTransactionLineId = line.JobOrderCancelLineId;
                                Uid.LastTransactionPersonId = Header.JobWorkerId;
                                if (JobOrderLine.ProductUidHeaderId == Uid.ProductUidHeaderId)
                                    Uid.Status = ProductUidStatusConstants.Cancel;
                                else
                                    Uid.Status = ProductUidStatusConstants.Receive;

                                Uid.CurrenctProcessId = Header.ProcessId;
                                Uid.CurrenctGodownId = Header.GodownId;
                                Uid.ModifiedBy = User.Identity.Name;
                                Uid.ModifiedDate = DateTime.Now;
                                Uid.ObjectState = Model.ObjectState.Modified;
                                //new ProductUidService(_unitOfWork).Update(Uid);
                                db.ProductUid.Add(Uid);

                            }
                        }

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
                            StockViewModel.DocHeaderId = Header.JobOrderCancelHeaderId;
                            StockViewModel.DocLineId = line.JobOrderCancelLineId;
                            StockViewModel.DocTypeId = Header.DocTypeId;
                            StockViewModel.StockHeaderDocDate = Header.DocDate;
                            StockViewModel.StockDocDate = Header.DocDate;
                            StockViewModel.DocNo = Header.DocNo;
                            StockViewModel.DivisionId = Header.DivisionId;
                            StockViewModel.SiteId = Header.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.PersonId = Header.JobWorkerId;
                            StockViewModel.ProductId = JobOrderLine.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = Header.GodownId;
                            StockViewModel.HeaderProcessId = Header.ProcessId;
                            StockViewModel.GodownId = (int)Header.GodownId;
                            StockViewModel.Remark = Header.Remark;
                            StockViewModel.Status = Header.Status;
                            StockViewModel.ProcessId = Header.ProcessId;
                            StockViewModel.LotNo = null;

                            if (temp != null)
                            {
                                StockViewModel.CostCenterId = temp.CostCenterId;
                                StockViewModel.Rate = temp.Rate;
                            }
                            StockViewModel.Qty_Iss = 0;
                            StockViewModel.Qty_Rec = line.Qty;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = JobOrderLine.Specification;
                            StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockViewModel.ProductUidId = line.ProductUidId;
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
                            StockProcessViewModel.DocHeaderId = Header.JobOrderCancelHeaderId;
                            StockProcessViewModel.DocLineId = line.JobOrderCancelLineId;
                            StockProcessViewModel.DocTypeId = Header.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                            StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                            StockProcessViewModel.DocNo = Header.DocNo;
                            StockProcessViewModel.DivisionId = Header.DivisionId;
                            StockProcessViewModel.SiteId = Header.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = Header.JobWorkerId;
                            StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = Header.GodownId;
                            StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                            StockProcessViewModel.GodownId = (int)Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = null;

                            if (temp != null)
                            {
                                StockProcessViewModel.CostCenterId = temp.CostCenterId;
                                StockProcessViewModel.Rate = temp.Rate;
                            }
                            StockProcessViewModel.Qty_Iss = line.Qty;
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = JobOrderLine.Specification;
                            StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockProcessViewModel.ProductUidId = line.ProductUidId;
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

                        line.ObjectState = Model.ObjectState.Added;
                        db.JobOrderCancelLine.Add(line);


                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        var BomPostList = _JobOrderCancelLineService.GetBomPostingDataForCancel(JobOrderLine.ProductId, JobOrderLine.Dimension1Id, JobOrderLine.Dimension2Id, JobOrderLine.Dimension3Id, JobOrderLine.Dimension4Id, Header.ProcessId, line.Qty, Header.DocTypeId).ToList();

                        foreach (var Litem in BomPostList)
                        {
                            JobOrderCancelBom BomPost = new JobOrderCancelBom();
                            BomPost.CreatedBy = User.Identity.Name;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.Dimension1Id = Litem.Dimension1Id;
                            BomPost.Dimension2Id = Litem.Dimension2Id;
                            BomPost.Dimension3Id = Litem.Dimension3Id;
                            BomPost.Dimension4Id = Litem.Dimension4Id;
                            BomPost.JobOrderCancelHeaderId = line.JobOrderCancelHeaderId;
                            BomPost.JobOrderCancelLineId = line.JobOrderCancelLineId;
                            BomPost.ModifiedBy = User.Identity.Name;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = Litem.ProductId;
                            BomPost.Qty = Convert.ToDecimal(Litem.Qty);
                            BomPost.ObjectState = Model.ObjectState.Added;

                            db.JobOrderCancelBom.Add(BomPost);
                            //new JobOrderCancelBomService(_unitOfWork).Create(BomPost);
                        }

                        Cnt = Cnt + 1;
                    }
                    
                     
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderCancelHeader.Add(Header);
                //new JobOrderCancelHeaderService(_unitOfWork).Update(Header);
                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyCancelMultipleDB(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobOrderCancelDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderCancelViewModels.FirstOrDefault().JobOrderCancelHeaderId), ref db);
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

                try
                {
                    JobOrderCancelDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderCancelViewModels.FirstOrDefault().JobOrderCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                if (BarCodeBased.Count() > 0)
                {
                    List<BarCodeSequenceViewModel> Sequence = new List<BarCodeSequenceViewModel>();

                    foreach (var item in BarCodeBased)
                        Sequence.Add(new BarCodeSequenceViewModel
                        {
                            JobOrderCancelHeaderId = item.JobOrderCancelHeaderId,
                            JobOrderLineId = item.JobOrderLineId,
                            ProductName = item.ProductName,
                            Qty = item.Qty,
                            BalanceQty = item.BalanceQty,
                            FirstBarCode = _JobOrderCancelLineService.GetFirstBarCodeForCancel(item.JobOrderLineId),
                        });

                    BarCodeSequenceListViewModel SquenceList = new BarCodeSequenceListViewModel();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    return PartialView("_Sequence", SquenceList);

                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                  
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));                


                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _SequencePost(BarCodeSequenceListViewModel vm)
        {
            List<BarCodeSequenceViewModel> Sequence = new List<BarCodeSequenceViewModel>();
            BarCodeSequenceListViewModel SquenceList = new BarCodeSequenceListViewModel();

            JobOrderCancelHeader Header = new JobOrderCancelHeaderService(_unitOfWork).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderCancelHeaderId);

            foreach (var item in vm.BarCodeSequenceViewModel)
            {
                BarCodeSequenceViewModel SequenceLine = new BarCodeSequenceViewModel();

                SequenceLine.JobOrderCancelHeaderId = item.JobOrderCancelHeaderId;
                SequenceLine.JobOrderLineId = item.JobOrderLineId;
                SequenceLine.ProductName = item.ProductName;
                SequenceLine.Qty = item.Qty;

                if (!string.IsNullOrEmpty(item.ProductUidIdName))
                {
                    var BarCodes = _JobOrderCancelLineService.GetPendingBarCodesList(SequenceLine.JobOrderLineId);

                    var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SequenceLine.ProductUidIds = Ids;
                }
                else
                {
                    var BarCodes = _JobOrderCancelLineService.GetPendingBarCodesList(SequenceLine.JobOrderLineId);

                    var temp = BarCodes.Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SequenceLine.ProductUidIds = Ids;
                }

                Sequence.Add(SequenceLine);

            }
            SquenceList.BarCodeSequenceViewModel = Sequence;
            return PartialView("_BarCodes", SquenceList);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _BarCodesPost(BarCodeSequenceListViewModel vm)
        {
            int Cnt = 0;
            int Pk = 0;
            List<JobOrderCancelLineViewModel> BarCodeBased = new List<JobOrderCancelLineViewModel>();
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            int Serial = _JobOrderCancelLineService.GetMaxSr(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderCancelHeaderId);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderCancelDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderCancelHeaderId), ref db);
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


            var JobOrderLineIds = vm.BarCodeSequenceViewModel.Select(m => m.JobOrderLineId).ToArray();

            var JobOrderLineRecords = (from p in db.JobOrderLine
                                       where JobOrderLineIds.Contains(p.JobOrderLineId)
                                       select p).ToList();
            var JobOrderHeaderIds = JobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                         where JobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                         select p).ToList();

            var SProductUids = string.Join(",", vm.BarCodeSequenceViewModel.Select(m => m.ProductUidIds));

            var ProductUids = SProductUids.Split(',').Select(Int32.Parse).ToArray();

            var ProductUidRecords = (from p in db.ProductUid
                                     where ProductUids.Contains(p.ProductUIDId)
                                     select p).ToList();

            var BalCostCenterRecords = (from L in db.JobOrderLine
                                        join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                                        from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                                        where JobOrderLineIds.Contains(L.JobOrderLineId)
                                        select new { CostCenterId = JobOrderHeaderTab.CostCenterId, Rate = L.Rate, JobOrderLineId = L.JobOrderLineId }).ToList();

            var BalanceQtyRecords = (from p in db.ViewJobOrderBalance
                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                     select new { BalQty = p.BalanceQty, JobOrderLineId = p.JobOrderLineId }).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobOrderCancelHeader Header = new JobOrderCancelHeaderService(_unitOfWork).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderCancelHeaderId);
                JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                foreach (var item in vm.BarCodeSequenceViewModel)
                {

                    //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(item.JobOrderLineId);
                    //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    JobOrderLine JobOrderLine = JobOrderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                    JobOrderHeader JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();

                    //var temp = (from L in db.JobOrderLine
                    //            join H in db.JobOrderHeader on L.JobOrderHeaderId equals H.JobOrderHeaderId into JobOrderHeaderTable
                    //            from JobOrderHeaderTab in JobOrderHeaderTable.DefaultIfEmpty()
                    //            where L.JobOrderLineId == item.JobOrderLineId
                    //            select new { CostCenterId = JobOrderHeaderTab.CostCenterId, Rate = L.Rate }).FirstOrDefault();

                    //decimal balqty = (from p in db.ViewJobOrderBalance
                    //                  where p.JobOrderLineId == item.JobOrderLineId
                    //                  select p.BalanceQty).FirstOrDefault();

                    var temp = (from p in BalCostCenterRecords
                                where p.JobOrderLineId == item.JobOrderLineId
                                select p).FirstOrDefault();

                    decimal balqty = (from L in BalanceQtyRecords
                                      where L.JobOrderLineId == item.JobOrderLineId
                                      select L.BalQty).FirstOrDefault();


                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {







                            JobOrderCancelLine line = new JobOrderCancelLine();

                            line.JobOrderCancelHeaderId = item.JobOrderCancelHeaderId;
                            line.JobOrderLineId = item.JobOrderLineId;
                            line.ProductUidId = BarCodes;
                            line.Qty = 1;
                            line.Sr = Serial++;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobOrderCancelLineId = Pk++;

                            if (LineStatus.ContainsKey(line.JobOrderLineId))
                            {
                                LineStatus[line.JobOrderLineId] = LineStatus[line.JobOrderLineId] + 1;
                            }
                            else
                            {
                                LineStatus.Add(line.JobOrderLineId, line.Qty);
                            }

                            if (JobOrderLine.ProductUidHeaderId.HasValue && JobOrderLine.ProductUidHeaderId.Value > 0)
                            {

                                //ProductUid Uid = new ProductUidService(_unitOfWork).Find(BarCodes);

                                ProductUid Uid = ProductUidRecords.Where(m => m.ProductUIDId == BarCodes).FirstOrDefault();

                                line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                                line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                                line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                                line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                                line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                                line.ProductUidStatus = Uid.Status;
                                line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                                line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;

                                if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                                {

                                    Uid.LastTransactionDocId = Header.JobOrderCancelHeaderId;
                                    Uid.LastTransactionDocDate = Header.DocDate;
                                    Uid.LastTransactionDocNo = Header.DocNo;
                                    Uid.LastTransactionDocTypeId = Header.DocTypeId;
                                    Uid.LastTransactionLineId = line.JobOrderCancelLineId;
                                    Uid.LastTransactionPersonId = Header.JobWorkerId;
                                    if (JobOrderLine.ProductUidHeaderId == Uid.ProductUidHeaderId)
                                        Uid.Status = ProductUidStatusConstants.Cancel;
                                    else
                                        Uid.Status = ProductUidStatusConstants.Receive;
                                    Uid.CurrenctProcessId = null;
                                    Uid.CurrenctGodownId = null;
                                    Uid.ModifiedBy = User.Identity.Name;
                                    Uid.ModifiedDate = DateTime.Now;
                                    Uid.ObjectState = Model.ObjectState.Modified;

                                    db.ProductUid.Add(Uid);
                                    //new ProductUidService(_unitOfWork).Update(Uid);
                                }


                            }

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
                                StockViewModel.DocHeaderId = Header.JobOrderCancelHeaderId;
                                StockViewModel.DocLineId = line.JobOrderCancelLineId;
                                StockViewModel.DocTypeId = Header.DocTypeId;
                                StockViewModel.StockHeaderDocDate = Header.DocDate;
                                StockViewModel.StockDocDate = Header.DocDate;
                                StockViewModel.DocNo = Header.DocNo;
                                StockViewModel.DivisionId = Header.DivisionId;
                                StockViewModel.SiteId = Header.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.PersonId = Header.JobWorkerId;
                                StockViewModel.ProductId = JobOrderLine.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = Header.GodownId;
                                StockViewModel.HeaderProcessId = Header.ProcessId;
                                StockViewModel.GodownId = (int)Header.GodownId;
                                StockViewModel.Remark = Header.Remark;
                                StockViewModel.Status = Header.Status;
                                StockViewModel.ProcessId = Header.ProcessId;
                                StockViewModel.LotNo = null;

                                if (temp != null)
                                {
                                    StockViewModel.CostCenterId = temp.CostCenterId;
                                    StockViewModel.Rate = temp.Rate;
                                }
                                StockViewModel.Qty_Iss = 0;
                                StockViewModel.Qty_Rec = line.Qty;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = JobOrderLine.Specification;
                                StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                                StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                                StockViewModel.ProductUidId = line.ProductUidId;
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
                                    return PartialView("_BarCodes", vm);
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
                                StockProcessViewModel.DocHeaderId = Header.JobOrderCancelHeaderId;
                                StockProcessViewModel.DocLineId = line.JobOrderCancelLineId;
                                StockProcessViewModel.DocTypeId = Header.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                                StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                                StockProcessViewModel.DocNo = Header.DocNo;
                                StockProcessViewModel.DivisionId = Header.DivisionId;
                                StockProcessViewModel.SiteId = Header.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.PersonId = Header.JobWorkerId;
                                StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                StockProcessViewModel.HeaderGodownId = Header.GodownId;
                                StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                                StockProcessViewModel.GodownId = (int)Header.GodownId;
                                StockProcessViewModel.Remark = Header.Remark;
                                StockProcessViewModel.Status = Header.Status;
                                StockProcessViewModel.ProcessId = Header.ProcessId;
                                StockProcessViewModel.LotNo = null;

                                if (temp != null)
                                {
                                    StockProcessViewModel.CostCenterId = temp.CostCenterId;
                                    StockProcessViewModel.Rate = temp.Rate;
                                }
                                StockProcessViewModel.Qty_Iss = line.Qty;
                                StockProcessViewModel.Qty_Rec = 0;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = JobOrderLine.Specification;
                                StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                                StockProcessViewModel.ProductUidId = line.ProductUidId;
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
                                    return PartialView("_BarCodes", vm);
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

                            line.ObjectState = Model.ObjectState.Added;
                            db.JobOrderCancelLine.Add(line);
                            // _JobOrderCancelLineService.Create(line);

                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            //Saving BOMPOST Data
                            var BomPostList = _JobOrderCancelLineService.GetBomPostingDataForCancel(JobOrderLine.ProductId, JobOrderLine.Dimension1Id, JobOrderLine.Dimension2Id, JobOrderLine.Dimension3Id, JobOrderLine.Dimension4Id, Header.ProcessId, line.Qty, Header.DocTypeId).ToList();

                            foreach (var Litem in BomPostList)
                            {
                                JobOrderCancelBom BomPost = new JobOrderCancelBom();
                                BomPost.CreatedBy = User.Identity.Name;
                                BomPost.CreatedDate = DateTime.Now;
                                BomPost.Dimension1Id = Litem.Dimension1Id;
                                BomPost.Dimension2Id = Litem.Dimension2Id;
                                BomPost.Dimension3Id = Litem.Dimension3Id;
                                BomPost.Dimension4Id = Litem.Dimension4Id;
                                BomPost.JobOrderCancelHeaderId = line.JobOrderCancelHeaderId;
                                BomPost.JobOrderCancelLineId = line.JobOrderCancelLineId;
                                BomPost.ModifiedBy = User.Identity.Name;
                                BomPost.ModifiedDate = DateTime.Now;
                                BomPost.ProductId = Litem.ProductId;
                                BomPost.Qty = Convert.ToDecimal(Litem.Qty);
                                BomPost.ObjectState = Model.ObjectState.Added;

                                db.JobOrderCancelBom.Add(BomPost);
                                //new JobOrderCancelBomService(_unitOfWork).Create(BomPost);
                            }

                        }
                    }
                }
                new JobOrderCancelHeaderService(_unitOfWork).Update(Header);
                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyCancelMultipleDB(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobOrderCancelDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderCancelHeaderId), ref db);
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
                    return PartialView("_BarCodes", vm);
                }

                try
                {
                    JobOrderCancelDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                   
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_BarCodes", vm);

        }

        [HttpGet]
        public ActionResult CreateLine(int Id, int sid)
        {
            return _Create(Id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int sid)
        {
            return _Create(Id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int sid)
        {
            return _Create(Id, sid);
        }

        public ActionResult _Create(int Id, int sid) //Id ==>Sale Order Header Id
        {
            JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(Id);
            JobOrderCancelLineViewModel svm = new JobOrderCancelLineViewModel();

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
            ViewBag.LineMode = "Create";
            svm.JobOrderCancelHeaderId = Id;
            svm.JobWorkerId = sid;
            svm.GodownId = header.GodownId;

            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }

            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobOrderCancelLineViewModel svm)
        {

            List<JobOrderCancelLine> CancelLines = new List<JobOrderCancelLine>();

            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (svm.JobOrderCancelLineId <= 0)
                    BeforeSave = JobOrderCancelDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderCancelHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobOrderCancelDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderCancelHeaderId, EventModeConstants.Edit), ref db);
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


            if (svm.JobOrderCancelLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.JobOrderCancelLineId <= 0 && BeforeSave && !EventException)
            {
                JobOrderCancelHeader temp = new JobOrderCancelHeaderService(_unitOfWork).Find(svm.JobOrderCancelHeaderId);


                decimal balqty = (from p in db.ViewJobOrderBalance
                                  where p.JobOrderLineId == svm.JobOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (ModelState.IsValid)
                {
                    int pk = 0;
                    int Sr = _JobOrderCancelLineService.GetMaxSr(svm.JobOrderCancelHeaderId);
                    JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(svm.JobOrderLineId);
                    JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    if (string.IsNullOrEmpty(svm.BarCodes))
                    {
                        JobOrderCancelLine CancelLine = new JobOrderCancelLine();
                        CancelLine.Remark = svm.Remark;
                        CancelLine.JobOrderCancelHeaderId = svm.JobOrderCancelHeaderId;
                        CancelLine.JobOrderLineId = svm.JobOrderLineId;
                        CancelLine.Qty = svm.Qty;
                        CancelLine.ProductUidId = svm.ProductUidId;
                        CancelLine.Sr = Sr++;
                        CancelLine.CreatedDate = DateTime.Now;
                        CancelLine.ModifiedDate = DateTime.Now;
                        CancelLine.CreatedBy = User.Identity.Name;
                        CancelLine.ModifiedBy = User.Identity.Name;
                        CancelLine.JobOrderCancelLineId = pk++;
                        CancelLines.Add(CancelLine);

                        if (CancelLine.ProductUidId.HasValue)
                        {

                            ProductUid Uid = new ProductUidService(_unitOfWork).Find(CancelLine.ProductUidId.Value);

                            CancelLine.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            CancelLine.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            CancelLine.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            CancelLine.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            CancelLine.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            CancelLine.ProductUidStatus = Uid.Status;
                            CancelLine.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            CancelLine.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                            if (temp.JobWorkerId == Uid.LastTransactionPersonId || temp.SiteId == 17)
                            {

                                Uid.LastTransactionDocId = temp.JobOrderCancelHeaderId;
                                Uid.LastTransactionDocDate = temp.DocDate;
                                Uid.LastTransactionDocNo = temp.DocNo;
                                Uid.LastTransactionDocTypeId = temp.DocTypeId;
                                Uid.LastTransactionPersonId = temp.JobWorkerId;

                                if (Uid.ProductUidHeaderId == JobOrderLine.ProductUidHeaderId)
                                    Uid.Status = ProductUidStatusConstants.Cancel;
                                else
                                    Uid.Status = ProductUidStatusConstants.Receive;

                                Uid.CurrenctProcessId = temp.ProcessId;
                                Uid.CurrenctGodownId = temp.GodownId;
                                Uid.ModifiedBy = User.Identity.Name;
                                Uid.ModifiedDate = DateTime.Now;
                                //new ProductUidService(_unitOfWork).Update(Uid);
                                Uid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(Uid);

                            }




                        }
                    }
                    else
                    {
                        foreach (var item in svm.BarCodes.Split(',').Select(Int32.Parse).ToList())
                        {
                            JobOrderCancelLine CancelLine = new JobOrderCancelLine();
                            CancelLine.Remark = svm.Remark;
                            CancelLine.JobOrderCancelHeaderId = svm.JobOrderCancelHeaderId;
                            CancelLine.JobOrderLineId = svm.JobOrderLineId;
                            CancelLine.ProductUidId = item;
                            CancelLine.Qty = 1;
                            CancelLine.Sr = Sr++;
                            CancelLine.CreatedDate = DateTime.Now;
                            CancelLine.ModifiedDate = DateTime.Now;
                            CancelLine.CreatedBy = User.Identity.Name;
                            CancelLine.ModifiedBy = User.Identity.Name;
                            CancelLine.JobOrderCancelLineId = pk++;
                            CancelLine.ProductUidId = item;


                            ProductUid Uid = new ProductUidService(_unitOfWork).Find(item);



                            CancelLine.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            CancelLine.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            CancelLine.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            CancelLine.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            CancelLine.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            CancelLine.ProductUidStatus = Uid.Status;
                            CancelLine.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            CancelLine.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                            if (temp.JobWorkerId == Uid.LastTransactionPersonId || temp.SiteId == 17)
                            {

                                Uid.LastTransactionDocId = temp.JobOrderCancelHeaderId;
                                Uid.LastTransactionDocDate = temp.DocDate;
                                Uid.LastTransactionDocNo = temp.DocNo;
                                Uid.LastTransactionDocTypeId = temp.DocTypeId;
                                Uid.LastTransactionPersonId = temp.JobWorkerId;
                                if (JobOrderLine.ProductUidHeaderId == Uid.ProductUidHeaderId)
                                    Uid.Status = ProductUidStatusConstants.Cancel;
                                else
                                    Uid.Status = ProductUidStatusConstants.Receive;
                                Uid.CurrenctProcessId = null;
                                Uid.CurrenctGodownId = null;
                                Uid.ModifiedBy = User.Identity.Name;
                                Uid.ModifiedDate = DateTime.Now;
                                Uid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(Uid);


                                CancelLines.Add(CancelLine);
                                //new ProductUidService(_unitOfWork).Update(Uid);

                            }
                        }
                    }


                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnCancel(svm.JobOrderLineId, svm.JobOrderCancelLineId, temp.DocDate, svm.Qty, ref db, true);




                    //if (JobOrderLine.ProductUidHeaderId.HasValue && JobOrderLine.ProductUidHeaderId.Value > 0 && !string.IsNullOrEmpty(svm.BarCodes))
                    //{


                    //    foreach (var item in svm.BarCodes.Split(',').Select(Int32.Parse).ToList())
                    //    {
                    //        ProductUid Uid = new ProductUidService(_unitOfWork).Find(item);
                    //        Uid.LastTransactionDocId = temp.JobOrderCancelHeaderId;
                    //        Uid.LastTransactionDocDate = temp.DocDate;
                    //        Uid.LastTransactionDocNo = temp.DocNo;
                    //        Uid.LastTransactionDocTypeId = temp.DocTypeId;
                    //        Uid.LastTransactionPersonId = temp.JobWorkerId;
                    //        Uid.Status = ProductUidStatusConstants.Cancel;
                    //        Uid.CurrenctProcessId = null;
                    //        Uid.CurrenctGodownId = null;
                    //        Uid.ModifiedBy = User.Identity.Name;
                    //        Uid.ModifiedDate = DateTime.Now;
                    //        new ProductUidService(_unitOfWork).Update(Uid);
                    //    }



                    //}


                    foreach (JobOrderCancelLine s in CancelLines)
                    {



                        //Posting in Stock
                        if (svm.JobOrderSettings.isPostedInStock)
                        {
                            StockViewModel StockViewModel = new StockViewModel();

                            StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                            StockViewModel.DocHeaderId = temp.JobOrderCancelHeaderId;
                            StockViewModel.DocLineId = s.JobOrderCancelLineId;
                            StockViewModel.DocTypeId = temp.DocTypeId;
                            StockViewModel.StockHeaderDocDate = temp.DocDate;
                            StockViewModel.StockDocDate = temp.DocDate;
                            StockViewModel.DocNo = temp.DocNo;
                            StockViewModel.DivisionId = temp.DivisionId;
                            StockViewModel.SiteId = temp.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.HeaderProcessId = null;
                            StockViewModel.PersonId = temp.JobWorkerId;
                            StockViewModel.ProductId = JobOrderLine.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = null;
                            StockViewModel.GodownId = temp.GodownId ?? 0;
                            StockViewModel.ProcessId = temp.ProcessId;
                            StockViewModel.LotNo = null;
                            StockViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                            StockViewModel.Qty_Iss = 0;
                            StockViewModel.Qty_Rec = s.Qty;
                            StockViewModel.Rate = JobOrderLine.Rate;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = JobOrderLine.Specification;
                            StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockViewModel.ProductUidId = s.ProductUidId; ;
                            StockViewModel.Remark = s.Remark;
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



                        //Posting in StockProcess
                        if (svm.JobOrderSettings.isPostedInStockProcess)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (temp.StockHeaderId != null && temp.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)temp.StockHeaderId;
                            }
                            else if (svm.JobOrderSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessViewModel.StockHeaderId = 0;
                            }


                            StockProcessViewModel.DocHeaderId = temp.JobOrderCancelHeaderId;
                            StockProcessViewModel.DocLineId = s.JobOrderCancelLineId;
                            StockProcessViewModel.DocTypeId = temp.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                            StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                            StockProcessViewModel.DocNo = temp.DocNo;
                            StockProcessViewModel.DivisionId = temp.DivisionId;
                            StockProcessViewModel.SiteId = temp.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.HeaderProcessId = null;
                            StockProcessViewModel.PersonId = temp.JobWorkerId;
                            StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = null;
                            StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                            StockProcessViewModel.ProcessId = temp.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                            StockProcessViewModel.Qty_Iss = s.Qty;
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.Rate = JobOrderLine.Rate;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = JobOrderLine.Specification;
                            StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockProcessViewModel.Remark = s.Remark;
                            StockProcessViewModel.ProductUidId = s.ProductUidId;
                            StockProcessViewModel.Status = temp.Status;
                            StockProcessViewModel.CreatedBy = temp.CreatedBy;
                            StockProcessViewModel.CreatedDate = DateTime.Now;
                            StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                ModelState.AddModelError("", StockProcessPostingError);
                                return PartialView("_Create", svm);
                            }

                            s.StockProcessId = StockProcessViewModel.StockProcessId;

                            if (svm.JobOrderSettings.isPostedInStock == false)
                            {
                                if (temp.StockHeaderId == null)
                                {
                                    temp.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                }
                            }
                        }

                        s.ObjectState = Model.ObjectState.Added;
                        db.JobOrderCancelLine.Add(s);
                        //_JobOrderCancelLineService.Create(s);



                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data
                        //Saving BOMPOST Data

                        var BomPostList = _JobOrderCancelLineService.GetBomPostingDataForCancel(JobOrderLine.ProductId, JobOrderLine.Dimension1Id, JobOrderLine.Dimension2Id, JobOrderLine.Dimension3Id, JobOrderLine.Dimension4Id, temp.ProcessId, s.Qty, temp.DocTypeId).ToList();

                        foreach (var item in BomPostList)
                        {
                            JobOrderCancelBom BomPost = new JobOrderCancelBom();
                            BomPost.CreatedBy = User.Identity.Name;
                            BomPost.CreatedDate = DateTime.Now;
                            BomPost.Dimension1Id = item.Dimension1Id;
                            BomPost.Dimension2Id = item.Dimension2Id;
                            BomPost.Dimension3Id = item.Dimension3Id;
                            BomPost.Dimension4Id = item.Dimension4Id;
                            BomPost.JobOrderCancelHeaderId = s.JobOrderCancelHeaderId;
                            BomPost.JobOrderCancelLineId = s.JobOrderCancelLineId;
                            BomPost.ModifiedBy = User.Identity.Name;
                            BomPost.ModifiedDate = DateTime.Now;
                            BomPost.ProductId = item.ProductId;
                            BomPost.Qty = Convert.ToDecimal(item.Qty);
                            BomPost.ObjectState = Model.ObjectState.Added;
                            db.JobOrderCancelBom.Add(BomPost);
                            //new JobOrderCancelBomService(_unitOfWork).Create(BomPost);
                        }



                    }





                    //JobOrderCancelHeader temp2 = new JobOrderCancelHeaderService(_unitOfWork).Find(s.JobOrderCancelHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderCancelHeader.Add(temp);
                    //new JobOrderCancelHeaderService(_unitOfWork).Update(temp);

                    try
                    {
                        JobOrderCancelDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderCancelHeaderId, EventModeConstants.Add), ref db);
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
                        JobOrderCancelDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderCancelHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderCancelHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,                      
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.JobOrderCancelHeaderId, sid = svm.JobWorkerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                JobOrderCancelHeader temp = new JobOrderCancelHeaderService(_unitOfWork).Find(svm.JobOrderCancelHeaderId);
                int status = temp.Status;

                JobOrderCancelLine s = _JobOrderCancelLineService.Find(svm.JobOrderCancelLineId);


                JobOrderCancelLine ExRec = new JobOrderCancelLine();
                ExRec = Mapper.Map<JobOrderCancelLine>(s);


                decimal balqty = (from p in db.ViewJobOrderBalance
                                  where p.JobOrderLineId == svm.JobOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }

                JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(s.JobOrderLineId);
                JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {

                        s.Remark = svm.Remark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                        new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnCancel(s.JobOrderLineId, s.JobOrderCancelLineId, temp.DocDate, s.Qty, ref db, true);
                       

                        if (s.StockId != null)
                        {
                            StockViewModel StockViewModel = new StockViewModel();
                            StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                            StockViewModel.StockId = s.StockId ?? 0;
                            StockViewModel.DocHeaderId = s.JobOrderCancelHeaderId;
                            StockViewModel.DocLineId = s.JobOrderCancelLineId;
                            StockViewModel.DocTypeId = temp.DocTypeId;
                            StockViewModel.StockHeaderDocDate = temp.DocDate;
                            StockViewModel.StockDocDate = temp.DocDate;
                            StockViewModel.DocNo = temp.DocNo;
                            StockViewModel.DivisionId = temp.DivisionId;
                            StockViewModel.SiteId = temp.SiteId;
                            StockViewModel.CurrencyId = null;
                            StockViewModel.HeaderProcessId = null;
                            StockViewModel.PersonId = temp.JobWorkerId;
                            StockViewModel.ProductId = JobOrderLine.ProductId;
                            StockViewModel.HeaderFromGodownId = null;
                            StockViewModel.HeaderGodownId = temp.GodownId;
                            StockViewModel.GodownId = temp.GodownId ?? 0;
                            StockViewModel.ProcessId = null;
                            StockViewModel.LotNo = JobOrderLine.LotNo;
                            StockViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                            StockViewModel.Qty_Iss = 0;
                            StockViewModel.Qty_Rec = s.Qty;
                            StockViewModel.Rate = JobOrderLine.Rate;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = JobOrderLine.Specification;
                            StockViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockViewModel.Remark = s.Remark;
                            StockViewModel.ProductUidId = s.ProductUidId;
                            StockViewModel.Status = temp.Status;
                            StockViewModel.CreatedBy = s.CreatedBy;
                            StockViewModel.CreatedDate = s.CreatedDate;
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




                        if (s.StockProcessId != null)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                            StockProcessViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                            StockProcessViewModel.StockProcessId = s.StockProcessId ?? 0;
                            StockProcessViewModel.DocHeaderId = s.JobOrderCancelHeaderId;
                            StockProcessViewModel.DocLineId = s.JobOrderCancelLineId;
                            StockProcessViewModel.DocTypeId = temp.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                            StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                            StockProcessViewModel.DocNo = temp.DocNo;
                            StockProcessViewModel.DivisionId = temp.DivisionId;
                            StockProcessViewModel.SiteId = temp.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.HeaderProcessId = null;
                            StockProcessViewModel.PersonId = temp.JobWorkerId;
                            StockProcessViewModel.ProductId = JobOrderLine.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = temp.GodownId;
                            StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                            StockProcessViewModel.ProcessId = null;
                            StockProcessViewModel.LotNo = JobOrderLine.LotNo;
                            StockProcessViewModel.CostCenterId = JobOrderHeader.CostCenterId;
                            StockProcessViewModel.Qty_Iss = s.Qty;
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.Rate = JobOrderLine.Rate;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = JobOrderLine.Specification;
                            StockProcessViewModel.Dimension1Id = JobOrderLine.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = JobOrderLine.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = JobOrderLine.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = JobOrderLine.Dimension4Id;
                            StockProcessViewModel.Remark = s.Remark;
                            StockProcessViewModel.ProductUidId = s.ProductUidId;
                            StockProcessViewModel.Status = temp.Status;
                            StockProcessViewModel.CreatedBy = s.CreatedBy;
                            StockProcessViewModel.CreatedDate = s.CreatedDate;
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

                    }

                    //_JobOrderCancelLineService.Update(s);
                    s.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderCancelLine.Add(s);


                    var Boms = (from p in db.JobOrderCancelBom
                                where p.JobOrderCancelLineId == s.JobOrderCancelLineId
                                select p).ToList();

                    foreach (var item in Boms)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobOrderCancelBom.Remove(item);
                        //new JobOrderBomService(_unitOfWork).Delete(item);
                    }



                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data
                    //Saving BOMPOST Data

                    var BomPostList = _JobOrderCancelLineService.GetBomPostingDataForCancel(JobOrderLine.ProductId, JobOrderLine.Dimension1Id, JobOrderLine.Dimension2Id, JobOrderLine.Dimension3Id, JobOrderLine.Dimension4Id, temp.ProcessId, s.Qty, temp.DocTypeId).ToList();

                    foreach (var item in BomPostList)
                    {
                        JobOrderCancelBom BomPost = new JobOrderCancelBom();
                        BomPost.CreatedBy = User.Identity.Name;
                        BomPost.CreatedDate = DateTime.Now;
                        BomPost.Dimension1Id = item.Dimension1Id;
                        BomPost.Dimension2Id = item.Dimension2Id;
                        BomPost.Dimension3Id = item.Dimension3Id;
                        BomPost.Dimension4Id = item.Dimension4Id;
                        BomPost.JobOrderCancelHeaderId = s.JobOrderCancelHeaderId;
                        BomPost.JobOrderCancelLineId = s.JobOrderCancelLineId;
                        BomPost.ModifiedBy = User.Identity.Name;
                        BomPost.ModifiedDate = DateTime.Now;
                        BomPost.ProductId = item.ProductId;
                        BomPost.Qty = Convert.ToDecimal(item.Qty);
                        BomPost.ObjectState = Model.ObjectState.Added;
                        db.JobOrderCancelBom.Add(BomPost);
                        //new JobOrderCancelBomService(_unitOfWork).Create(BomPost);
                    }


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = s,
                    });

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    //new JobOrderCancelHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderCancelHeader.Add(temp);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobOrderCancelDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderCancelHeaderId, s.JobOrderCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
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
                        JobOrderCancelDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderCancelHeaderId, s.JobOrderCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderCancelHeaderId,
                        DocLineId = s.JobOrderCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));


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





        [HttpGet]
        private ActionResult _Modify(int id)
        {
            JobOrderCancelLineViewModel temp = _JobOrderCancelLineService.GetJobOrderCancelLine(id);

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

            JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(temp.JobOrderCancelHeaderId);

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            

            var JobOrderLine = new JobOrderLineService(_unitOfWork).Find(temp.JobOrderLineId);
          
            if (temp.ProductUidId.HasValue)
            {
                ViewBag.BarCodeGenerated = true;
                temp.BarCodes = temp.ProductUidId.ToString();
            }
            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
           
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


        [HttpGet]
        public ActionResult _Delete(int id)
        {
            JobOrderCancelLineViewModel temp = _JobOrderCancelLineService.GetJobOrderCancelLine(id);

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

            JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(temp.JobOrderCancelHeaderId);

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);


            var JobOrderLine = new JobOrderLineService(_unitOfWork).Find(temp.JobOrderLineId);

            if (temp.ProductUidId.HasValue)
            {
                ViewBag.BarCodeGenerated = true;
                temp.BarCodes = temp.ProductUidId.ToString();
            }
            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
            
            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobOrderCancelLineViewModel vm)
        {

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderCancelDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobOrderCancelHeaderId, vm.JobOrderCancelLineId), ref db);
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

                int? StockId = 0;
                int? StockProcessId = 0;
                string ValidationMsg = "";

                JobOrderCancelLine JobOrderCancelLine = (from p in db.JobOrderCancelLine
                                                         where p.JobOrderCancelLineId == vm.JobOrderCancelLineId
                                                         select p).FirstOrDefault();

                JobOrderCancelHeader CancelHEader = new JobOrderCancelHeaderService(_unitOfWork).Find(JobOrderCancelLine.JobOrderCancelHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobOrderCancelLine>(JobOrderCancelLine),
                });

                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnCancel(JobOrderCancelLine.JobOrderLineId, JobOrderCancelLine.JobOrderCancelLineId, CancelHEader.DocDate, 0, ref db, true);

                if (JobOrderCancelLine.ProductUidId.HasValue && JobOrderCancelLine.ProductUidId.Value > 0)
                {
                    var ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)JobOrderCancelLine.ProductUidId, "Job Order Cancel-" + CancelHEader.JobOrderCancelHeaderId.ToString());

                    ProductUid ProductUid = new ProductUidService(_unitOfWork).Find(JobOrderCancelLine.ProductUidId.Value);

                    if (!(JobOrderCancelLine.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && JobOrderCancelLine.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || CancelHEader.SiteId==17)
                    {

                        if (CancelHEader.DocNo != ProductUid.LastTransactionDocNo || CancelHEader.DocTypeId != ProductUid.LastTransactionDocTypeId)
                        {
                            ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                            ViewBag.LineMode = "Delete";
                            return PartialView("_Create", vm);
                        }


                        ProductUid.LastTransactionDocDate = JobOrderCancelLine.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = JobOrderCancelLine.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = JobOrderCancelLine.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = JobOrderCancelLine.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = JobOrderCancelLine.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = JobOrderCancelLine.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = JobOrderCancelLine.ProductUidCurrentProcessId;
                        ProductUid.Status = JobOrderCancelLine.ProductUidStatus;

                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(ProductUid);

                        //new ProductUidService(_unitOfWork).Update(ProductUid);
                    }
                    ViewBag.BarCodeGenerated = true;
                }


                //_JobOrderCancelLineService.Delete(vm.JobOrderCancelLineId);

                StockId = JobOrderCancelLine.StockId;
                StockProcessId = JobOrderCancelLine.StockProcessId;

                JobOrderCancelLine.ObjectState = Model.ObjectState.Deleted;
                db.JobOrderCancelLine.Remove(JobOrderCancelLine);

                if (StockId != null)
                {
                    new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
                }

                if (StockProcessId != null)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB((int)StockProcessId, ref db, true);
                }

                //JobOrderLine OrderLine = new JobOrderLineService(_unitOfWork).Find(JobOrderLine.JobOrderLineId);
                //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(OrderLine.JobOrderHeaderId);



                JobOrderCancelHeader header = new JobOrderCancelHeaderService(_unitOfWork).Find(JobOrderCancelLine.JobOrderCancelHeaderId);
                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    //new JobOrderCancelHeaderService(_unitOfWork).Update(header);
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderCancelHeader.Add(header);
                }

                var CancelBoms = (from p in db.JobOrderCancelBom
                                  where p.JobOrderCancelLineId == vm.JobOrderCancelLineId
                                  select p).ToList();

                foreach (var item in CancelBoms)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobOrderCancelBom.Remove(item);
                    //new JobOrderCancelBomService(_unitOfWork).Delete(item);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobOrderCancelDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobOrderCancelLine.JobOrderCancelHeaderId, JobOrderCancelLine.JobOrderCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
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
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobOrderCancelDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobOrderCancelLine.JobOrderCancelHeaderId, JobOrderCancelLine.JobOrderCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = CancelHEader.DocTypeId,
                    DocId = CancelHEader.JobOrderCancelHeaderId,
                    DocLineId = JobOrderCancelLine.JobOrderCancelLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                   
                    DocNo = CancelHEader.DocNo,
                    DocDate = CancelHEader.DocDate,
                    xEModifications = Modifications,
                    DocStatus=CancelHEader.Status,
                }));

            }
            return Json(new { success = true });
        }


        public JsonResult GetPendingOrders(int JobWorkerId, string term, int Limit)
        {
            return Json(new JobOrderHeaderService(_unitOfWork).GetPendingJobOrdersWithPatternMatch(JobWorkerId, term, Limit).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new JobOrderLineService(_unitOfWork).GetLineDetailForCancel(LineId));
        }

        public JsonResult GetBarCodes(int Id)
        {
            return Json(_JobOrderCancelLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobOrders(int id, string term)//DocTypeId
        {
            //var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            //var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //string DocTypes = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobOrderCancelLineService.GetPendingJobOrders("", term, id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobOrderProducts(int id, string term)//DocTypeId
        {
            //var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            //var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //string DocTypes = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobOrderCancelLineService.GetPendingProductsForJobOrderCancel("", term, id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOrderLineForUid(int UId, int CancelHeaderId)
        {
            var JobOrderCancelHeader = new JobOrderCancelHeaderService(_unitOfWork).Find(CancelHeaderId);

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var JobOrderLin = (from p in db.ProductUid
                               where p.ProductUIDId == UId
                               join t in db.ProductUidHeader on p.ProductUidHeaderId equals t.ProductUidHeaderId
                               join t2 in db.JobOrderLine on t.ProductUidHeaderId equals t2.ProductUidHeaderId
                               join t3 in db.JobOrderHeader on t2.JobOrderHeaderId equals t3.JobOrderHeaderId
                               where t3.SiteId == SiteId && t3.DivisionId == DivisionId && t3.ProcessId == JobOrderCancelHeader.ProcessId
                               select t2).FirstOrDefault();


            if (JobOrderLin != null && JobOrderLin.ProductUidHeaderId != null)
            {

               //var JOLine = _JobOrderCancelLineService.GetOrderLineForUidMain(UId);
               var JOLine = _JobOrderCancelLineService.GetOrderLineForUidMain(UId, CancelHeaderId);
                if (JOLine == null)
                    return Json(false, JsonRequestBehavior.AllowGet);
                else
                    return Json(JOLine, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var JOLine = _JobOrderCancelLineService.GetOrderLineForUidBranch(UId);
                if (JOLine == null)
                    return Json(false, JsonRequestBehavior.AllowGet);
                else
                    return Json(JOLine, JsonRequestBehavior.AllowGet);

            }

        }

        public JsonResult CheckDuplicateJobOrder(int LineId, int CancelHeaderId)
        {
            return Json(_JobOrderCancelLineService.CheckForDuplicateJobOrder(LineId, CancelHeaderId), JsonRequestBehavior.AllowGet);
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

    }

}
