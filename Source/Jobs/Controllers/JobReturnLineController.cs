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
using JobReturnDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobReturnLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;
        IJobReturnLineService _JobReturnLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReturnLineController(IJobReturnLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReturnLineService = SaleOrder;
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
            var p = _JobReturnLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForReceipt(int id, int sid)
        {
            JobReturnLineFilterViewModel vm = new JobReturnLineFilterViewModel();
            vm.JobReturnHeaderId = id;
            JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.JobWorkerId = sid;
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForOrders(int id, int sid)
        {
            JobReturnLineFilterViewModel vm = new JobReturnLineFilterViewModel();
            vm.JobReturnHeaderId = id;
            JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.JobWorkerId = sid;
            return PartialView("_OrderFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobReturnLineFilterViewModel vm)
        {
            List<JobReturnLineViewModel> temp = _JobReturnLineService.GetJobReceivesForFilters(vm).ToList();
            JobReturnMasterDetailModel svm = new JobReturnMasterDetailModel();
            svm.JobReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(JobReturnLineFilterViewModel vm)
        {
            List<JobReturnLineViewModel> temp = _JobReturnLineService.GetJobOrderForFilters(vm).ToList();
            JobReturnMasterDetailModel svm = new JobReturnMasterDetailModel();
            svm.JobReturnLineViewModel = temp;
            return PartialView("_OrderResults", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobReturnMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobReturnLineService.GetMaxSr(vm.JobReturnLineViewModel.FirstOrDefault().JobReturnHeaderId);

            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobReturnLineViewModel> BarCodeBased = new List<JobReturnLineViewModel>();

            int[] JobReceiveLinesIds = vm.JobReturnLineViewModel.Select(m => m.JobReceiveLineId).ToArray();

            var JobReceiveRecords = (from p in db.JobReceiveLine
                                     where JobReceiveLinesIds.Contains(p.JobReceiveLineId)
                                     select p).ToList();

            var JobReceiveBalanceQtys = (from p in db.ViewJobReceiveBalance
                                         where JobReceiveLinesIds.Contains(p.JobReceiveLineId)
                                         select new
                                         {
                                             p.BalanceQty,
                                             p.JobReceiveLineId
                                         }).ToList();

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReturnDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobReturnLineViewModel.FirstOrDefault().JobReturnHeaderId), ref db);
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
                JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(vm.JobReturnLineViewModel.FirstOrDefault().JobReturnHeaderId);

                JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                foreach (var item in vm.JobReturnLineViewModel)
                {
                    //decimal balqty = (from p in db.ViewJobReceiveBalance
                    //                  where p.JobReceiveLineId== item.JobReceiveLineId
                    //                  select p.BalanceQty).FirstOrDefault();

                    decimal balqty = JobReceiveBalanceQtys.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault().BalanceQty;

                    JobReceiveLine Receive = JobReceiveRecords.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault();

                    if (Receive.ProductUidId.HasValue && Receive.ProductUidId.Value > 0 && item.Qty > 0 && item.Qty <= balqty)
                        BarCodeBased.Add(item);

                    if (item.Qty > 0 && item.Qty <= balqty && !Receive.ProductUidId.HasValue)
                    {
                        JobReturnLine Line = new JobReturnLine();
                        Line.JobReturnHeaderId = item.JobReturnHeaderId;
                        Line.JobReceiveLineId = item.JobReceiveLineId;
                        Line.Qty = item.Qty;
                        Line.Sr = Serial++;
                        Line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        Line.DealUnitId = item.DealUnitId;
                        Line.UnitConversionMultiplier = item.UnitConversionMultiplier;

                        if (Receive.Weight != 0)
                        {
                            decimal UnitWeight = Receive.Weight / Receive.Qty;
                            Line.Weight = UnitWeight * Line.Qty;
                        }

                        Line.Remark = item.Remark;
                        Line.CreatedDate = DateTime.Now;
                        Line.ModifiedDate = DateTime.Now;
                        Line.CreatedBy = User.Identity.Name;
                        Line.ModifiedBy = User.Identity.Name;

                        LineStatus.Add(Line.JobReceiveLineId, Line.Qty);

                        //JobReceiveLine JobReceiveLine = new JobReceiveLineService(_unitOfWork).Find(Line.JobReceiveLineId);
                        //var receipt = new JobReceiveLineService(_unitOfWork).Find(item.JobReceiveLineId );



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
                            StockViewModel.DocHeaderId = Header.JobReturnHeaderId;
                            StockViewModel.DocLineId = Line.JobReceiveLineId;
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
                            StockViewModel.GodownId = Header.GodownId;
                            StockViewModel.Remark = Header.Remark;
                            StockViewModel.Status = Header.Status;
                            StockViewModel.ProcessId = Header.ProcessId;
                            StockViewModel.LotNo = null;
                            StockViewModel.CostCenterId = null;
                            StockViewModel.Qty_Iss = Line.Qty;
                            StockViewModel.Qty_Rec = 0;
                            StockViewModel.Rate = null;
                            StockViewModel.ExpiryDate = null;
                            StockViewModel.Specification = item.Specification;
                            StockViewModel.Dimension1Id = item.Dimension1Id;
                            StockViewModel.Dimension2Id = item.Dimension2Id;
                            StockViewModel.Dimension3Id = item.Dimension3Id;
                            StockViewModel.Dimension4Id = item.Dimension4Id;
                            StockViewModel.CreatedBy = User.Identity.Name;
                            StockViewModel.ProductUidId = Receive.ProductUidId;
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


                            Line.StockId = StockViewModel.StockId;
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
                            StockProcessViewModel.DocHeaderId = Header.JobReturnHeaderId;
                            StockProcessViewModel.DocLineId = Line.JobReceiveLineId;
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
                            StockProcessViewModel.GodownId = Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = null;
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = Line.Qty;
                            StockProcessViewModel.Rate = null;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = item.Specification;
                            StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = item.Dimension4Id;
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
                                    Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                }
                            }


                            Line.StockProcessId = StockProcessViewModel.StockProcessId;
                        }



                        //if (Receive.ProductUidId.HasValue && Receive.ProductUidId > 0)
                        //{
                        //    ProductUid Uid = new ProductUidService(_unitOfWork).Find(Receive.ProductUidId.Value);



                        //    Line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                        //    Line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                        //    Line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                        //    Line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                        //    Line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                        //    Line.ProductUidStatus = Uid.Status;
                        //    Line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                        //    Line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                        //    Uid.LastTransactionDocId = Header.JobReturnHeaderId;
                        //    Uid.LastTransactionDocDate = Header.DocDate;
                        //    Uid.LastTransactionDocNo = Header.DocNo;
                        //    Uid.LastTransactionDocTypeId = Header.DocTypeId;
                        //    Uid.LastTransactionPersonId = Header.JobWorkerId;
                        //    Uid.Status = ProductUidStatusConstants.Issue;
                        //    Uid.CurrenctProcessId = Header.ProcessId;
                        //    var Site = new SiteService(_unitOfWork).FindByPerson(Header.JobWorkerId);
                        //    if (Site != null)
                        //        Uid.CurrenctGodownId = Site.DefaultGodownId;
                        //    else
                        //        Uid.CurrenctGodownId = null;

                        //    Uid.ModifiedBy = User.Identity.Name;
                        //    Uid.ModifiedDate = DateTime.Now;
                        //    new ProductUidService(_unitOfWork).Update(Uid);

                        //}



                        Line.ObjectState = Model.ObjectState.Added;
                        db.JobReturnLine.Add(Line);
                        //_JobReturnLineService.Create(Line);

                        Cnt = Cnt + 1;

                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobReturnHeader.Add(Header);
                //new JobReturnHeaderService(_unitOfWork).Update(Header);
                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReturnMultiple(LineStatus, Header.DocDate, ref db);
                new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyReturnMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobReturnDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobReturnLineViewModel.FirstOrDefault().JobReturnHeaderId), ref db);
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
                    JobReturnDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobReturnLineViewModel.FirstOrDefault().JobReturnHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                if (BarCodeBased.Count() > 0)
                {
                    List<JobReturnBarCodeSequenceViewModel> Sequence = new List<JobReturnBarCodeSequenceViewModel>();
                    List<JobReturnBarCodeSequenceViewModel> GSequence = new List<JobReturnBarCodeSequenceViewModel>();
                    foreach (var item in BarCodeBased)
                        Sequence.Add(new JobReturnBarCodeSequenceViewModel
                        {
                            JobReturnHeaderId = item.JobReturnHeaderId,
                            JobReceiveLineId = item.JobReceiveLineId,
                            ProductName = item.ProductName,
                            Qty = item.Qty,
                            BalanceQty = item.GoodsReceiptBalQty,
                            //FirstBarCode = _JobReturnLineService.GetFirstBarCodeForReturn(item.JobReceiveLineId),
                        });


                    GSequence = (from p in Sequence
                                 orderby p.ProductName
                                 group p by p.ProductName into g
                                 select new JobReturnBarCodeSequenceViewModel
                                 {
                                     JobReturnHeaderId = g.Max(m => m.JobReturnHeaderId),
                                     ProductName = g.Key,
                                     SJobRecLineIds = string.Join(",", g.Select(m => m.JobReceiveLineId).ToList()),
                                     Qty = g.Sum(m => m.Qty),
                                     BalanceQty = g.Sum(m => m.BalanceQty),
                                     //FirstBarCode = _JobReturnLineService.GetFirstBarCodeForReturn(g.Select(m => m.JobReceiveLineId).ToArray()),
                                 }).ToList();


                    JobReturnBarCodeSequenceListViewModel SquenceList = new JobReturnBarCodeSequenceListViewModel();
                    SquenceList.JobReturnBarCodeSequenceViewModel = GSequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    return PartialView("_Sequence", SquenceList);

                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobReturnHeaderId,
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
        public ActionResult _SequencePost(JobReturnBarCodeSequenceListViewModel vm)
        {
            List<JobReturnBarCodeSequenceViewModel> Sequence = new List<JobReturnBarCodeSequenceViewModel>();
            JobReturnBarCodeSequenceListViewModel SquenceList = new JobReturnBarCodeSequenceListViewModel();

            JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(vm.JobReturnBarCodeSequenceViewModel.FirstOrDefault().JobReturnHeaderId);

            foreach (var item in vm.JobReturnBarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                JobReturnBarCodeSequenceViewModel SequenceLine = new JobReturnBarCodeSequenceViewModel();

                SequenceLine.JobReturnHeaderId = item.JobReturnHeaderId;
                SequenceLine.JobReceiveLineId = item.JobReceiveLineId;
                SequenceLine.ProductName = item.ProductName;
                SequenceLine.SJobRecLineIds = item.SJobRecLineIds;


                if (!string.IsNullOrEmpty(item.ProductUidIdName))
                {
                    var BarCodes = _JobReturnLineService.GetPendingBarCodesList(SequenceLine.SJobRecLineIds);

                    var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SequenceLine.ProductUidIds = Ids;
                }
                else
                {
                    var BarCodes = _JobReturnLineService.GetPendingBarCodesList(SequenceLine.SJobRecLineIds);

                    var temp = BarCodes.Take((int)item.Qty);
                    string Ids = string.Join(",", temp.Select(m => m.Id));
                    SequenceLine.ProductUidIds = Ids;
                }
                SequenceLine.Qty = SequenceLine.ProductUidIds.Split(',').Count();
                Sequence.Add(SequenceLine);

            }
            SquenceList.JobReturnBarCodeSequenceViewModelPost = Sequence;
            return PartialView("_BarCodes", SquenceList);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _BarCodesPost(JobReturnBarCodeSequenceListViewModel vm)
        {
            int Cnt = 0;
            int Serial = _JobReturnLineService.GetMaxSr(vm.JobReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobReturnHeaderId);

            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            //s[] JobReceiveLinesIds = vm.JobReturnBarCodeSequenceViewModel.Select(m => m.SJobRecLineIds).ToList().ToArray();

            string Temp = string.Join(",", vm.JobReturnBarCodeSequenceViewModelPost.Select(m => m.SJobRecLineIds).ToList());

            int[] Ids = Temp.Split(',').Select(Int32.Parse).ToArray();

            var JobReceiveRecords = (from p in db.JobReceiveLine
                                     where Ids.Contains(p.JobReceiveLineId)
                                     select p).ToList();

            var JobReceiveBalanceQtys = (from p in db.ViewJobReceiveBalance
                                         where Ids.Contains(p.JobReceiveLineId)
                                         select new
                                         {
                                             p.BalanceQty,
                                             p.JobReceiveLineId,
                                         }).ToList();

            var JobOrders = (from p in db.JobReceiveLine
                             join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                             where Ids.Contains(p.JobReceiveLineId)
                             select new { Order = t, Receive = p.JobReceiveLineId }).ToList();



            var BarCode = string.Join(",", vm.JobReturnBarCodeSequenceViewModelPost.Select(m => m.ProductUidIds).ToList());
            int[] BarCodeIds = BarCode.Split(',').Select(Int32.Parse).ToArray();

            var BarCodeRecords = (from p in db.ProductUid
                                  where BarCodeIds.Contains(p.ProductUIDId)
                                  select p).ToList();

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReturnDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobReturnHeaderId), ref db);
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
                JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(vm.JobReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobReturnHeaderId);
                var Site = new SiteService(_unitOfWork).FindByPerson(Header.JobWorkerId);
                JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                foreach (var item in vm.JobReturnBarCodeSequenceViewModelPost)
                {
                    //decimal balqty = (from p in db.ViewJobReceiveBalance
                    //                  where p.JobReceiveLineId== item.JobReceiveLineId
                    //                  select p.BalanceQty).FirstOrDefault();

                    decimal balqty = (from p in JobReceiveBalanceQtys
                                      where item.SJobRecLineIds.Contains(p.JobReceiveLineId.ToString())
                                      select p.BalanceQty).ToList().Sum();



                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {
                            JobReceiveLine Receive = JobReceiveRecords.Where(m => m.ProductUidId == BarCodes).FirstOrDefault();

                            JobReturnLine Line = new JobReturnLine();
                            Line.JobReturnHeaderId = item.JobReturnHeaderId;
                            Line.JobReceiveLineId = Receive.JobReceiveLineId;
                            Line.Qty = 1;
                            Line.Sr = Serial++;

                            if (Receive.Weight != 0)
                            {
                                decimal UnitWeight = Receive.Weight / Receive.Qty;
                                Line.Weight = UnitWeight * Line.Qty;
                            }

                            Line.DealQty = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.UnitConversionMultiplier;
                            Line.DealUnitId = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.DealUnitId;
                            Line.UnitConversionMultiplier = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.UnitConversionMultiplier;
                            Line.CreatedDate = DateTime.Now;
                            Line.ModifiedDate = DateTime.Now;
                            Line.CreatedBy = User.Identity.Name;
                            Line.ModifiedBy = User.Identity.Name;

                            LineStatus.Add(Line.JobReceiveLineId, Line.Qty);


                            JobReceiveLine JobReceiveLine = JobReceiveRecords.Where(m => m.JobReceiveLineId == Line.JobReceiveLineId).FirstOrDefault();
                            //var receipt = new JobReceiveLineService(_unitOfWork).Find(item.JobReceiveLineId );



                            ProductUid Uid = BarCodeRecords.Where(m => m.ProductUIDId == BarCodes).FirstOrDefault();

                            Line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            Line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            Line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            Line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            Line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            Line.ProductUidStatus = Uid.Status;
                            Line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            Line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;

                            if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                            {

                                Uid.LastTransactionDocId = Header.JobReturnHeaderId;
                                Uid.LastTransactionDocDate = Header.DocDate;
                                Uid.LastTransactionDocNo = Header.DocNo;
                                Uid.LastTransactionDocTypeId = Header.DocTypeId;
                                Uid.LastTransactionLineId = Line.JobReturnLineId;
                                Uid.LastTransactionPersonId = Header.JobWorkerId;
                                Uid.Status = ProductUidStatusConstants.Return;
                                Uid.CurrenctProcessId = Header.ProcessId;

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
                                StockViewModel.DocHeaderId = Header.JobReturnHeaderId;
                                StockViewModel.DocLineId = Line.JobReceiveLineId;
                                StockViewModel.DocTypeId = Header.DocTypeId;
                                StockViewModel.StockHeaderDocDate = Header.DocDate;
                                StockViewModel.StockDocDate = Header.DocDate;
                                StockViewModel.DocNo = Header.DocNo;
                                StockViewModel.DivisionId = Header.DivisionId;
                                StockViewModel.SiteId = Header.SiteId;
                                StockViewModel.CurrencyId = null;
                                StockViewModel.PersonId = Header.JobWorkerId;
                                StockViewModel.ProductId = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.ProductId;
                                StockViewModel.HeaderFromGodownId = null;
                                StockViewModel.HeaderGodownId = Header.GodownId;
                                StockViewModel.HeaderProcessId = Header.ProcessId;
                                StockViewModel.GodownId = Header.GodownId;
                                StockViewModel.Remark = Header.Remark;
                                StockViewModel.Status = Header.Status;
                                StockViewModel.ProcessId = Header.ProcessId;
                                StockViewModel.LotNo = null;
                                StockViewModel.CostCenterId = null;
                                StockViewModel.Qty_Iss = Line.Qty;
                                StockViewModel.Qty_Rec = 0;
                                StockViewModel.Rate = null;
                                StockViewModel.ExpiryDate = null;
                                StockViewModel.Specification = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Specification;
                                StockViewModel.Dimension1Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension1Id;
                                StockViewModel.Dimension2Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension2Id;
                                StockViewModel.Dimension3Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension3Id;
                                StockViewModel.Dimension4Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension4Id;
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
                                    Header.StockHeaderId = StockViewModel.StockHeaderId;
                                }


                                Line.StockId = StockViewModel.StockId;
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
                                StockProcessViewModel.DocHeaderId = Header.JobReturnHeaderId;
                                StockProcessViewModel.DocLineId = Line.JobReceiveLineId;
                                StockProcessViewModel.DocTypeId = Header.DocTypeId;
                                StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                                StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                                StockProcessViewModel.DocNo = Header.DocNo;
                                StockProcessViewModel.DivisionId = Header.DivisionId;
                                StockProcessViewModel.SiteId = Header.SiteId;
                                StockProcessViewModel.CurrencyId = null;
                                StockProcessViewModel.PersonId = Header.JobWorkerId;
                                StockProcessViewModel.ProductId = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.ProductId;
                                StockProcessViewModel.HeaderFromGodownId = null;
                                StockProcessViewModel.HeaderGodownId = Header.GodownId;
                                StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                                StockProcessViewModel.GodownId = Header.GodownId;
                                StockProcessViewModel.Remark = Header.Remark;
                                StockProcessViewModel.Status = Header.Status;
                                StockProcessViewModel.ProcessId = Header.ProcessId;
                                StockProcessViewModel.LotNo = null;
                                StockProcessViewModel.CostCenterId = null;
                                StockProcessViewModel.Qty_Iss = 0;
                                StockProcessViewModel.Qty_Rec = Line.Qty;
                                StockProcessViewModel.Rate = null;
                                StockProcessViewModel.ExpiryDate = null;
                                StockProcessViewModel.Specification = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Specification;
                                StockProcessViewModel.Dimension1Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension1Id;
                                StockProcessViewModel.Dimension2Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension2Id;
                                StockProcessViewModel.Dimension3Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension3Id;
                                StockProcessViewModel.Dimension4Id = JobOrders.Where(m => m.Receive == Line.JobReceiveLineId).FirstOrDefault().Order.Dimension4Id;
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
                                        Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                                    }
                                }


                                Line.StockProcessId = StockProcessViewModel.StockProcessId;
                            }



                            Line.ObjectState = Model.ObjectState.Added;

                            db.JobReturnLine.Add(Line);
                            //_JobReturnLineService.Create(Line);

                            Cnt = Cnt + 1;

                        }
                    }

                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobReturnHeader.Add(Header);
                //new JobReturnHeaderService(_unitOfWork).Update(Header);
                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReturnMultiple(LineStatus, Header.DocDate, ref db);
                new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyReturnMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobReturnDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobReturnHeaderId), ref db);
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
                    JobReturnDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobReturnBarCodeSequenceViewModelPost.FirstOrDefault().JobReturnHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                   
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }


        private void PrepareViewBag(JobReturnLineViewModel vm)
        {
            //ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id, int sid, bool? WithProductUid)
        {
            return _Create(id, sid, WithProductUid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, int sid, bool? WithProductUid)
        {
            return _Create(id, sid, WithProductUid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, int sid, bool? WithProductUid)
        {
            return _Create(id, sid, WithProductUid);
        }
        public ActionResult _Create(int Id, int sid, bool? WithProductUid) //Id ==>Sale Order Header Id
        {
            JobReturnHeader H = new JobReturnHeaderService(_unitOfWork).Find(Id);
            JobReturnLineViewModel s = new JobReturnLineViewModel();

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.JobReturnHeaderId = H.JobReturnHeaderId;
            s.JobReturnHeaderDocNo = H.DocNo;
            s.JobWorkerId = sid;
            s.GodownId = H.GodownId;
            ViewBag.LineMode = "Create";

            //PrepareViewBag(null);

            if (WithProductUid == true)
            {
                return PartialView("_CreateForProductUid", s);
            }
            else
            {
                return PartialView("_Create", s);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobReturnLineViewModel svm)
        {

            bool BeforeSave = true;

            if (svm.JobReturnLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            try
            {

                if (svm.JobReturnLineId <= 0)
                    BeforeSave = JobReturnDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReturnDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReturnHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.JobReturnLineId <= 0 && BeforeSave && !EventException)
            {
                JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(svm.JobReturnHeaderId);

                decimal balqty = (from p in db.ViewJobReceiveBalanceForInvoice
                                  where p.JobReceiveLineId == svm.JobReceiveLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Receive Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }

                if (svm.JobReceiveLineId <= 0)
                {
                    ModelState.AddModelError("JobGoodsLineId", "Job Invoice field is required");
                }

                if (svm.DealQty <= 0)
                {
                    ModelState.AddModelError("DealQty", "DealQty field is required");
                }

                if (ModelState.IsValid)
                {
                    JobReturnLine Line = Mapper.Map<JobReturnLineViewModel, JobReturnLine>(svm);

                    if (svm.JobReceiveSettings.isPostedInStock)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                        StockViewModel.DocHeaderId = Header.JobReturnHeaderId;
                        StockViewModel.DocLineId = Line.JobReturnLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = Header.ProcessId;
                        StockViewModel.PersonId = Header.JobWorkerId;
                        StockViewModel.ProductId = svm.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.GodownId = Header.GodownId;
                        StockViewModel.ProcessId = Header.ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = Line.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Dimension3Id = svm.Dimension3Id;
                        StockViewModel.Dimension4Id = svm.Dimension4Id;
                        StockViewModel.Remark = Line.Remark;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.CreatedBy = Header.CreatedBy;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = Header.ModifiedBy;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }

                        Line.StockId = StockViewModel.StockId;

                        if (Header.StockHeaderId == null)
                        {
                            Header.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                    }



                    if (svm.JobReceiveSettings.isPostedInStockProcess)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                        if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                        }
                        else if (svm.JobReceiveSettings.isPostedInStock)//If Stok Header is already posted during stock posting then this statement will Execute.So theat Stock Header will not generate again.
                        {
                            StockProcessViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessViewModel.StockHeaderId = 0;
                        }


                        StockProcessViewModel.DocHeaderId = Header.JobReturnHeaderId;
                        StockProcessViewModel.DocLineId = Line.JobReturnLineId;
                        StockProcessViewModel.DocTypeId = Header.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                        StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                        StockProcessViewModel.DocNo = Header.DocNo;
                        StockProcessViewModel.DivisionId = Header.DivisionId;
                        StockProcessViewModel.SiteId = Header.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                        StockProcessViewModel.PersonId = Header.JobWorkerId;
                        StockProcessViewModel.ProductId = svm.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = Header.GodownId;
                        StockProcessViewModel.GodownId = Header.GodownId;
                        StockProcessViewModel.ProcessId = Header.ProcessId;
                        StockProcessViewModel.LotNo = null;
                        StockProcessViewModel.CostCenterId = null;
                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = Line.Qty;
                        StockProcessViewModel.Rate = null;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = svm.Specification;
                        StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = svm.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = svm.Dimension4Id;
                        StockProcessViewModel.Remark = Line.Remark;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.Status = Header.Status;
                        StockProcessViewModel.CreatedBy = Header.CreatedBy;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = Header.ModifiedBy;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }

                        Line.StockProcessId = StockProcessViewModel.StockProcessId;

                        if (svm.JobReceiveSettings.isPostedInStock == false)
                        {
                            if (Header.StockHeaderId == null)
                            {
                                Header.StockHeaderId = StockProcessViewModel.StockHeaderId;
                            }
                        }
                    }


                    Line.Sr = _JobReturnLineService.GetMaxSr(svm.JobReturnHeaderId);
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedDate = DateTime.Now;
                    Line.CreatedBy = User.Identity.Name;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ObjectState = Model.ObjectState.Added;

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReturn(Line.JobReceiveLineId, Line.JobReturnLineId, Header.DocDate, Line.Qty, ref db);
                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnReturn(Line.JobReceiveLineId, Line.JobReturnLineId, Header.DocDate, Line.Qty, ref db);




                    var JobReceiveLine = new JobReceiveLineService(_unitOfWork).Find(Line.JobReceiveLineId);

                    if (JobReceiveLine.ProductUidId.HasValue && JobReceiveLine.ProductUidId > 0)
                    {
                        ProductUid Uid = new ProductUidService(_unitOfWork).Find(JobReceiveLine.ProductUidId.Value);



                        Line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                        Line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                        Line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                        Line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                        Line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                        Line.ProductUidStatus = Uid.Status;
                        Line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                        Line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;



                        if (Header.JobWorkerId == Uid.LastTransactionPersonId || Header.SiteId == 17)
                        {

                            Uid.LastTransactionDocId = Header.JobReturnHeaderId;
                            Uid.LastTransactionDocDate = Header.DocDate;
                            Uid.LastTransactionDocNo = Header.DocNo;
                            Uid.LastTransactionDocTypeId = Header.DocTypeId;
                            Uid.LastTransactionLineId = Line.JobReturnLineId;
                            Uid.LastTransactionPersonId = Header.JobWorkerId;
                            Uid.Status = ProductUidStatusConstants.Return;
                            Uid.CurrenctProcessId = Header.ProcessId;

                            var Site = new SiteService(_unitOfWork).FindByPerson(Header.JobWorkerId);
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

                    }

                    //_JobReturnLineService.Create(Line);
                    Line.ObjectState = Model.ObjectState.Added;
                    db.JobReturnLine.Add(Line);

                    if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                    {
                        Header.ModifiedBy = User.Identity.Name;
                        Header.ModifiedDate = DateTime.Now;
                        Header.Status = (int)StatusConstants.Modified;
                    }



                    Header.ObjectState = Model.ObjectState.Modified;
                    db.JobReturnHeader.Add(Header);
                    //new JobReturnHeaderService(_unitOfWork).Update(Header);

                    try
                    {
                        JobReturnDocEvents.onLineSaveEvent(this, new JobEventArgs(Line.JobReturnHeaderId, Line.JobReturnLineId, EventModeConstants.Add), ref db);
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
                        JobReturnDocEvents.afterLineSaveEvent(this, new JobEventArgs(Line.JobReturnHeaderId, Line.JobReturnLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.JobReturnHeaderId,
                        DocLineId = Line.JobReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = Header.DocNo,
                        DocDate = Header.DocDate,
                        DocStatus=Header.Status,
                    }));

                    return RedirectToAction("_Create", new { id = Line.JobReturnHeaderId, sid = svm.JobWorkerId });
                }
                return PartialView("_Create", svm);


            }
            else if (BeforeSave && !EventException)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                JobReturnHeader Header = new JobReturnHeaderService(_unitOfWork).Find(svm.JobReturnHeaderId);
                int status = Header.Status;
                StringBuilder logstring = new StringBuilder();

                JobReturnLine Line = db.JobReturnLine.Find(svm.JobReturnLineId);


                JobReturnLine ExRec = new JobReturnLine();
                ExRec = Mapper.Map<JobReturnLine>(Line);


                decimal balqty = (from p in db.ViewJobReceiveBalanceForInvoice
                                  where p.JobReceiveLineId == svm.JobReceiveLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + Line.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Receive Qty");
                }

                if (svm.DealQty <= 0)
                {
                    ModelState.AddModelError("DealQty", "DealQty field is required");
                }

                if (ModelState.IsValid)
                {
                    Line.Remark = svm.Remark;
                    Line.Qty = svm.Qty;
                    Line.DealQty = svm.DealQty;
                    Line.Weight = svm.Weight ?? 0;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ModifiedDate = DateTime.Now;

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReturn(Line.JobReceiveLineId, Line.JobReturnLineId, Header.DocDate, Line.Qty, ref db);
                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnReturn(Line.JobReceiveLineId, Line.JobReturnLineId, Header.DocDate, Line.Qty, ref db);

                    Line.ObjectState = Model.ObjectState.Modified;
                    db.JobReturnLine.Add(Line);
                    //_JobReturnLineService.Update(Line);

                    if (Line.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                        StockViewModel.StockId = Line.StockId ?? 0;
                        StockViewModel.DocHeaderId = Header.JobReturnHeaderId;
                        StockViewModel.DocLineId = Line.JobReceiveLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.HeaderProcessId = Header.ProcessId;
                        StockViewModel.PersonId = Header.JobWorkerId;
                        StockViewModel.ProductId = svm.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.GodownId = Header.GodownId;
                        StockViewModel.ProcessId = Header.ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = svm.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = svm.Specification;
                        StockViewModel.Dimension1Id = svm.Dimension1Id;
                        StockViewModel.Dimension2Id = svm.Dimension2Id;
                        StockViewModel.Remark = Line.Remark;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.CreatedBy = Header.CreatedBy;
                        StockViewModel.CreatedDate = Header.CreatedDate;
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


                    if (Line.StockProcessId != null)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                        StockProcessViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                        StockProcessViewModel.StockProcessId = Line.StockProcessId ?? 0;
                        StockProcessViewModel.DocHeaderId = Header.JobReturnHeaderId;
                        StockProcessViewModel.DocLineId = Line.JobReceiveLineId;
                        StockProcessViewModel.DocTypeId = Header.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                        StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                        StockProcessViewModel.DocNo = Header.DocNo;
                        StockProcessViewModel.DivisionId = Header.DivisionId;
                        StockProcessViewModel.SiteId = Header.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                        StockProcessViewModel.PersonId = Header.JobWorkerId;
                        StockProcessViewModel.ProductId = svm.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = Header.GodownId;
                        StockProcessViewModel.GodownId = Header.GodownId;
                        StockProcessViewModel.ProcessId = Header.ProcessId;
                        StockProcessViewModel.LotNo = null;
                        StockProcessViewModel.CostCenterId = null;
                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = svm.Qty;
                        StockProcessViewModel.Rate = null;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = svm.Specification;
                        StockProcessViewModel.Dimension1Id = svm.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = svm.Dimension2Id;
                        StockProcessViewModel.Remark = Line.Remark;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.Status = Header.Status;
                        StockProcessViewModel.CreatedBy = Header.CreatedBy;
                        StockProcessViewModel.CreatedDate = Header.CreatedDate;
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


                    if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                    {
                        Header.Status = (int)StatusConstants.Modified;
                        Header.ModifiedDate = DateTime.Now;
                        Header.ModifiedBy = User.Identity.Name;
                        Header.ObjectState = Model.ObjectState.Modified;
                        db.JobReturnHeader.Add(Header);
                        //new JobReturnHeaderService(_unitOfWork).Update(Header);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = Line,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReturnDocEvents.onLineSaveEvent(this, new JobEventArgs(Line.JobReturnHeaderId, Line.JobReturnLineId, EventModeConstants.Edit), ref db);
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
                        JobReturnDocEvents.afterLineSaveEvent(this, new JobEventArgs(Line.JobReturnHeaderId, Line.JobReturnLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    //Saving the Activity Log


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.JobReturnHeaderId,
                        DocLineId = Line.JobReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = Header.DocNo,
                        xEModifications = Modifications,
                        DocDate = Header.DocDate,
                        DocStatus=Header.Status,
                    }));

                    //End of Saving the Activity Log


                    return Json(new { success = true });
                }
                return PartialView("_Create", svm);
            }
            else
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
            JobReturnLineViewModel temp = _JobReturnLineService.GetJobReturnLine(id);

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

            PrepareViewBag(temp);

            JobReturnHeader H = new JobReturnHeaderService(_unitOfWork).Find(temp.JobReturnHeaderId);
            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

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
            JobReturnLineViewModel temp = _JobReturnLineService.GetJobReturnLine(id);

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

            JobReturnHeader H = new JobReturnHeaderService(_unitOfWork).Find(temp.JobReturnHeaderId);
            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            JobReturnLineViewModel temp = _JobReturnLineService.GetJobReturnLine(id);

            PrepareViewBag(temp);
            JobReturnHeader H = new JobReturnHeaderService(_unitOfWork).Find(temp.JobReturnHeaderId);
            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            if (temp == null)
            {
                return HttpNotFound();
            }
            ViewBag.LineMode = "Detail";
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobReturnLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReturnDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobReturnHeaderId, vm.JobReturnLineId), ref db);
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
                int JobReceiveLineId = 0;
                JobReturnLine JobReturnLin = db.JobReturnLine.Find(vm.JobReturnLineId);
                JobReturnHeader header = new JobReturnHeaderService(_unitOfWork).Find(JobReturnLin.JobReturnHeaderId);
                StockId = JobReturnLin.StockId;
                JobReceiveLineId = JobReturnLin.JobReceiveLineId;
                LogList.Add(new LogTypeViewModel
                {
                    ExObj = JobReturnLin,
                });

                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReturn(JobReturnLin.JobReceiveLineId, JobReturnLin.JobReturnLineId, header.DocDate, 0, ref db);
                new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnReturn(JobReturnLin.JobReceiveLineId, JobReturnLin.JobReturnLineId, header.DocDate, 0, ref db);


                var JobReceiveLine = new JobReceiveLineService(_unitOfWork).Find(JobReceiveLineId);

                if (JobReceiveLine.ProductUidId.HasValue)
                {
                    //Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues(JobReceiveLine.ProductUidId.Value, "Job Return-" + vm.JobReturnHeaderId.ToString());

                    ProductUid ProductUid = new ProductUidService(_unitOfWork).Find(JobReceiveLine.ProductUidId.Value);

                    if (!(JobReturnLin.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && JobReturnLin.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || header.SiteId == 17)
                    {

                        if (header.DocNo != ProductUid.LastTransactionDocNo || header.DocTypeId != ProductUid.LastTransactionDocTypeId)
                        {
                            ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                            PrepareViewBag(vm);
                            ViewBag.LineMode = "Delete";
                            return PartialView("_Create", vm);
                        }

                        ProductUid.LastTransactionDocDate = JobReturnLin.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = JobReturnLin.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = JobReturnLin.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = JobReturnLin.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = JobReturnLin.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = JobReturnLin.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = JobReturnLin.ProductUidCurrentProcessId;
                        ProductUid.Status = JobReturnLin.ProductUidStatus;

                        //new ProductUidService(_unitOfWork).Update(ProductUid);
                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(ProductUid);

                    }
                }

                //_JobReturnLineService.Delete(JobReturnLin);
                JobReturnLin.ObjectState = Model.ObjectState.Deleted;
                db.JobReturnLine.Remove(JobReturnLin);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new JobReturnHeaderService(_unitOfWork).Update(header);
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobReturnHeader.Add(header);
                }

                if (StockId != null)
                {
                    new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobReturnDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobReturnLin.JobReturnHeaderId, JobReturnLin.JobReturnLineId), ref db);
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
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobReturnDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobReturnLin.JobReturnHeaderId, JobReturnLin.JobReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.JobReturnHeaderId,
                    DocLineId = JobReturnLin.JobReturnLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                  
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus=header.Status,
                }));

            }

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





        public JsonResult GetJobReceipts(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobReturnLineService.GetPendingJobReceiptHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJobOrders(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_JobReturnLineService.GetPendingJobOrderHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_JobReturnLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingReceipts(int JobWorkerId, string term, int Limit)
        {
            return Json(new JobReceiveHeaderService(_unitOfWork).GetPendingJobReceivesWithPatternMatch(JobWorkerId, term, Limit).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            var temp = new JobReceiveLineService(_unitOfWork).GetJobReceiveDetailBalance(LineId);
            if (temp != null)
                return Json(temp);
            else
                return Json(false);
        }

        public JsonResult GetProductUidDetail(string ProductUidName, int JobWorkerId, int ProcId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from L in db.JobReceiveLine
                        join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                        from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                        join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                        from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                        join Pu in db.ProductUid on (JobOrderLineTab.ProductUidHeaderId == null ? JobOrderLineTab.ProductUidId : L.ProductUidId) equals Pu.ProductUIDId into ProductUidTable
                        from ProductUidTab in ProductUidTable.DefaultIfEmpty()
                        where ProductUidTab.ProductUidName == ProductUidName && JobReceiveHeaderTab.ProcessId == ProcId && JobReceiveHeaderTab.SiteId == SiteId
                        && JobReceiveHeaderTab.DivisionId == DivisionId && JobReceiveHeaderTab.JobWorkerId == JobWorkerId
                        orderby JobReceiveHeaderTab.DocDate
                        select new
                        {
                            ProductUidId = ProductUidTab.ProductUIDId,
                            JobReceiveLineId = L.JobReceiveLineId,
                            JobReceiveDocNo = JobReceiveHeaderTab.DocNo,
                            Success = (JobReceiveHeaderTab.JobWorkerId == JobWorkerId ? true : false),
                            ProdUidHeaderId = JobOrderLineTab.ProductUidHeaderId,
                        }).ToList().Last();

            if (temp != null)
            {
                return Json(temp);
            }
            else
            {
                return Json(new { Success = false });
            }

        }

        public JsonResult GetBarCodes(string Id)
        {
            return Json(_JobReturnLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }


    }

}
