using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Jobs.Helpers;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using System.Data.Entity;
using DocumentEvents;
using CustomEventArgs;
using JobReceiveQADocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobReceiveQALineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;

        IJobReceiveQALineService _JobReceiveQALineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReceiveQALineController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockService = new StockService(unitOfWork);
            _JobReceiveQALineService = new JobReceiveQALineService(db, unitOfWork);
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
            var p = _JobReceiveQALineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }



        public ActionResult _ForReceive(int id, int sid)
        {
            JobReceiveQALineFilterViewModel vm = new JobReceiveQALineFilterViewModel();
            vm.JobReceiveQAHeaderId = id;
            vm.JobWorkerId = sid;
            JobReceiveQAHeader Header = new JobReceiveQAHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobReceiveQALineFilterViewModel vm)
        {
            List<JobReceiveQALineViewModel> temp = _JobReceiveQALineService.GetJobReceiveLineForMultiSelect(vm).ToList();
            JobReceiveQAMasterDetailModel svm = new JobReceiveQAMasterDetailModel();
            svm.JobReceiveQALineViewModel = temp.Where(m => m.ProductUidId == null).ToList();
            JobReceiveQAHeader Header = new JobReceiveQAHeaderService(db).Find(vm.JobReceiveQAHeaderId);
            JobReceiveQASettings settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            if (temp.Where(m => m.ProductUidId != null).Any())
            {
                List<JobReceiveQALineViewModel> Sequence = new List<JobReceiveQALineViewModel>();
                Sequence = temp.Where(m => m.ProductUidId != null).ToList();
                foreach (var item in Sequence)
                    item.JobInspectionType = JobReceiveTypeConstants.ProductUid;

                HttpContext.Session["UIQUID" + vm.JobReceiveQAHeaderId] = Sequence;
            }

            if (!temp.Where(m => m.ProductUidId == null).Any())
            {
                if (temp.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    List<BarCodeSequenceViewModelForQA> Sequence = new List<BarCodeSequenceViewModelForQA>();

                    var Grouping = (from p in temp.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobReceiveHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForQA
                        {
                            JobReceiveQAHeaderId = item.Max(m => m.JobReceiveQAHeaderId),
                            //JobReceiveQARequestLineId = item.JobReceiveQARequestLineId,
                            JobRecLineIds = string.Join(",", item.Select(m => m.JobReceiveLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobReceiveQAType = JobReceiveTypeConstants.ProductUid,
                            JobReceiveHeaderId = item.Max(m => m.JobReceiveHeaderId),
                            // FirstBarCode = _JobReceiveQALineService.GetFirstBarCodeForCancel(item.JobReceiveQARequestLineId),
                        });

                    BarCodeSequenceListViewModelForQA SquenceList = new BarCodeSequenceListViewModelForQA();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQUID" + vm.JobReceiveQAHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

            }

            return PartialView("_Results", svm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobReceiveQAMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobReceiveQALineService.GetMaxSr(vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId);
            List<JobReceiveQALineViewModel> LineStatus = new List<JobReceiveQALineViewModel>();
            List<JobReceiveQALineViewModel> BarCodeBasedBranch = new List<JobReceiveQALineViewModel>();

            BarCodeBasedBranch = (List<JobReceiveQALineViewModel>)HttpContext.Session["UIQUID" + vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId];


            HttpContext.Session.Remove("UIQUID" + vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId);


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveQADocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobReceiveQAHeader Header = new JobReceiveQAHeaderService(db).Find(vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId);

                JobReceiveQASettings Settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                var JobReceiveLineIds = vm.JobReceiveQALineViewModel.Select(m => m.JobReceiveLineId).ToArray();


                var JobReceiveLineBalanceQtyRecords = (from p in db.ViewJobReceiveBalanceForQA.AsNoTracking()
                                                       where JobReceiveLineIds.Contains(p.JobReceiveLineId)
                                                       select new { BalQty = p.BalanceQty, LineId = p.JobReceiveLineId }).ToList();

                var JobReceiveLineRecords = (from p in db.JobReceiveLine.Include(m => m.JobOrderLine).AsNoTracking()
                                             where JobReceiveLineIds.Contains(p.JobReceiveLineId)
                                             select p).ToList();

                var JobReceiveHeaderIds = JobReceiveLineRecords.Select(m => m.JobReceiveHeaderId).ToArray();

                var JobReceiveHeaderRecords = (from p in db.JobReceiveHeader
                                               where JobReceiveHeaderIds.Contains(p.JobReceiveHeaderId)
                                               select p).ToList();

                foreach (var item in vm.JobReceiveQALineViewModel.Where(m => m.Qty > 0))
                {
                    var balqty = JobReceiveLineBalanceQtyRecords.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().BalQty;

                    JobReceiveLine JobReceiveLine = JobReceiveLineRecords.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault();
                    JobReceiveHeader JobReceiveHeader = JobReceiveHeaderRecords.Where(m => m.JobReceiveHeaderId == JobReceiveLine.JobReceiveHeaderId).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty && JobReceiveLine.ProductUidId == null)
                    {

                        JobReceiveQALine line = new JobReceiveQALine();
                        line.JobReceiveQALineId = -Cnt;
                        line.JobReceiveQAHeaderId = item.JobReceiveQAHeaderId;
                        line.JobReceiveLineId = item.JobReceiveLineId;
                        line.ProductUidId = JobReceiveLine.ProductUidId;
                        line.Qty = item.Qty;
                        line.InspectedQty = item.InspectedQty;
                        line.UnitConversionMultiplier = JobReceiveLine.JobOrderLine.UnitConversionMultiplier;
                        line.DealQty = line.Qty * line.UnitConversionMultiplier;
                        line.Weight = item.Weight;
                        line.Marks = item.Marks;
                        line.Remark = item.Remark;
                        line.Sr = Serial++;

                        LineStatus.Add(Mapper.Map<JobReceiveQALineViewModel>(line));

                        _JobReceiveQALineService.Create(line, User.Identity.Name);

                        Cnt = Cnt + 1;

                    }
                }

                //new JobReceiveQAHeaderService(_unitOfWork).Update(Header);
                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }

                new JobReceiveQAHeaderService(db).Update(Header, User.Identity.Name);
                new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyQAMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobReceiveQADocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId), ref db);
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
                    return PartialView("_Results", vm);
                }

                try
                {
                    JobReceiveQADocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                if (BarCodeBasedBranch != null && BarCodeBasedBranch.Count() > 0)
                {
                    List<BarCodeSequenceViewModelForQA> Sequence = new List<BarCodeSequenceViewModelForQA>();

                    var Grouping = (from p in BarCodeBasedBranch.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobReceiveHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForQA
                        {
                            JobReceiveQAHeaderId = item.Max(m => m.JobReceiveQAHeaderId),
                            //JobReceiveQARequestLineId = item.JobReceiveQARequestLineId,
                            JobRecLineIds = string.Join(",", item.Select(m => m.JobReceiveLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobReceiveQAType = JobReceiveTypeConstants.ProductUid,
                            JobReceiveHeaderId = item.Max(m => m.JobReceiveHeaderId),
                            // FirstBarCode = _JobReceiveQALineService.GetFirstBarCodeForCancel(item.JobReceiveQARequestLineId),
                        });

                    BarCodeSequenceListViewModelForQA SquenceList = new BarCodeSequenceListViewModelForQA();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    HttpContext.Session.Remove("UIQUID" + vm.JobReceiveQALineViewModel.FirstOrDefault().JobReceiveQAHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobReceiveQAHeaderId,
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
        public ActionResult _SequencePost2(BarCodeSequenceListViewModelForQA vm)
        {
            List<BarCodeSequenceViewModelForQA> Seq = new List<BarCodeSequenceViewModelForQA>();
            BarCodeSequenceListViewModelForQA SquLis = new BarCodeSequenceListViewModelForQA();

            JobReceiveQAHeader Header = new JobReceiveQAHeaderService(db).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobReceiveQAHeaderId);

            var LineIds = vm.BarCodeSequenceViewModel.Select(m => m.JobReceiveLineId).ToArray();


            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                if (item.JobReceiveQAType == JobReceiveTypeConstants.ProductUid)
                {
                    string jOLIDs = item.JobRecLineIds;

                    BarCodeSequenceViewModelForQA SeqLi = new BarCodeSequenceViewModelForQA();

                    SeqLi.JobReceiveQAHeaderId = item.JobReceiveQAHeaderId;
                    SeqLi.JobRecLineIds = item.JobRecLineIds;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobReceiveQAType = item.JobReceiveQAType;
                    SeqLi.JobReceiveHeaderId = item.JobReceiveHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobReceiveQALineService.GetPendingBarCodesList(SeqLi.JobRecLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobReceiveQALineService.GetPendingBarCodesList(SeqLi.JobRecLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    Seq.Add(SeqLi);
                }
            }
            SquLis.BarCodeSequenceViewModelPost = Seq;
            return PartialView("_BarCodes", SquLis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _BarCodesPost(BarCodeSequenceListViewModelForQA vm)
        {

            int Cnt = 0;
            int Pk = 0;
            List<JobReceiveQALineViewModel> LineStatus = new List<JobReceiveQALineViewModel>();
            int Serial = _JobReceiveQALineService.GetMaxSr(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveQAHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveQADocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveQAHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            var JobReceiveLineIds = vm.BarCodeSequenceViewModelPost.Select(m => m.JobRecLineIds.Split(',').Select(Int32.Parse));

            List<int> RecLine = new List<int>();

            foreach (var item in JobReceiveLineIds)
                foreach (var item2 in item)
                {
                    RecLine.Add(item2);
                }


            var JobReceiveLineRecords = (from p in db.JobReceiveLine.Include(m => m.JobOrderLine).AsNoTracking()
                                         where RecLine.Contains(p.JobReceiveLineId)
                                         select p).ToList();

            var ProductUidJobReceiveLineIds = string.Join(",", vm.BarCodeSequenceViewModelPost.Where(m => !string.IsNullOrEmpty(m.JobRecLineIds)).Select(m => m.JobRecLineIds));

            int[] ProdUidJobReceiveLineIds;

            ProdUidJobReceiveLineIds = string.IsNullOrEmpty(ProductUidJobReceiveLineIds) ? new int[0] : ProductUidJobReceiveLineIds.Split(',').Select(Int32.Parse).ToArray();

            var ProdUidJobReceiveLineRecords = (from p in db.ViewJobReceiveBalanceForQA
                                                join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                                                where ProdUidJobReceiveLineIds.Contains(p.JobReceiveLineId)
                                                select t).ToList();
            var ProdUidJobReceiveHeaderIds = ProdUidJobReceiveLineRecords.GroupBy(m => m.JobReceiveHeaderId).Select(m => m.Key).ToArray();

            var ProdUidJobReceiveHeaderRecords = (from p in db.JobReceiveHeader
                                                  where ProdUidJobReceiveHeaderIds.Contains(p.JobReceiveHeaderId)
                                                  select p).ToList();

            var BalanceQtyRecords = (from p in db.ViewJobReceiveBalanceForQA
                                     where RecLine.Contains(p.JobReceiveLineId)
                                     select new { BalQty = p.BalanceQty, JobReceiveLineId = p.JobReceiveLineId }).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobReceiveQAHeader Header = new JobReceiveQAHeaderService(db).Find(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveQAHeaderId);

                foreach (var item in vm.BarCodeSequenceViewModelPost)
                {

                    JobReceiveLine JobReceiveLine = new JobReceiveLine();
                    JobReceiveHeader JobReceiveHeader = new JobReceiveHeader();

                    decimal balqty = 0;

                    if (item.JobReceiveQAType == JobReceiveTypeConstants.ProductUid)
                    {
                        var LineIds = item.JobRecLineIds.Split(',').Select(Int32.Parse).ToArray();
                        balqty = (from L in ProdUidJobReceiveLineRecords
                                  where LineIds.Contains(L.JobReceiveLineId)
                                  select L.Qty).Sum();
                    }


                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {
                            if (item.JobReceiveQAType == JobReceiveTypeConstants.ProductUid)
                            {
                                JobReceiveLine = ProdUidJobReceiveLineRecords.Where(m => m.ProductUidId == BarCodes && m.JobReceiveHeaderId == item.JobReceiveHeaderId).FirstOrDefault();
                                JobReceiveHeader = ProdUidJobReceiveHeaderRecords.Where(m => m.JobReceiveHeaderId == JobReceiveLine.JobReceiveHeaderId).FirstOrDefault();
                            }

                            JobReceiveQALine line = new JobReceiveQALine();

                            line.JobReceiveQAHeaderId = item.JobReceiveQAHeaderId;

                            if (item.JobReceiveQAType == JobReceiveTypeConstants.ProductUid)
                            {
                                line.JobReceiveLineId = JobReceiveLine.JobReceiveLineId;
                            }
                            line.ProductUidId = BarCodes;
                            line.Qty = 1;
                            line.UnitConversionMultiplier = JobReceiveLine.JobOrderLine.UnitConversionMultiplier;
                            line.DealQty = line.Qty * line.UnitConversionMultiplier;
                            line.Weight = JobReceiveLine.Weight;
                            line.InspectedQty = 1;
                            line.Sr = Serial++;
                            line.JobReceiveQALineId = Pk++;

                            LineStatus.Add(Mapper.Map<JobReceiveQALineViewModel>(line));

                            _JobReceiveQALineService.Create(line, User.Identity.Name);

                            //new JobReceiveQALineStatusService(_unitOfWork).CreateLineStatus(line.JobReceiveQALineId, ref db, true);


                            Cnt = Cnt + 1;
                        }
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                }

                new JobReceiveQAHeaderService(db).Update(Header, User.Identity.Name);
                new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyQAMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobReceiveQADocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveQAHeaderId), ref db);
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
                    return PartialView("_BarCodes", vm);
                }

                try
                {
                    JobReceiveQADocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobReceiveQAHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                return Json(new { success = true });

            }
            return PartialView("_BarCodes", vm);

        }

        private void PrepareViewBag(JobReceiveQALineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            var temp = new JobReceiveQAHeaderService(db).GetJobReceiveQAHeader(vm.JobReceiveQAHeaderId);
            ViewBag.DocNo = temp.DocTypeName + "-" + temp.DocNo;
        }

        [HttpGet]
        public ActionResult CreateLine(int Id, int JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int JobWorkerId)
        {
            return _Create(Id, JobWorkerId);
        }

        public ActionResult _Create(int Id, int JobWorkerId) //Id ==> Header Id
        {
            JobReceiveQAHeader H = new JobReceiveQAHeaderService(db).Find(Id);
            JobReceiveQALineViewModel s = new JobReceiveQALineViewModel();

            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.JobReceiveQAHeaderId = Id;
            s.JobReceiveQADocNo = H.DocNo;
            s.JobWorkerId = JobWorkerId;
            PrepareViewBag(s);
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
        public ActionResult _CreatePost(JobReceiveQALineViewModel svm)
        {
            JobReceiveQALine s = Mapper.Map<JobReceiveQALineViewModel, JobReceiveQALine>(svm);
            JobReceiveQAHeader temp = new JobReceiveQAHeaderService(db).Find(s.JobReceiveQAHeaderId);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (svm.JobReceiveQALineId <= 0)
                    BeforeSave = JobReceiveQADocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReceiveQALineId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReceiveQADocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobReceiveQALineId, EventModeConstants.Edit), ref db);

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


            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (svm.JobReceiveQALineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.QAQty <= 0)
                ModelState.AddModelError("QAQty", "The QAQty field is required");

            if (svm.InspectedQty <= 0)
                ModelState.AddModelError("InspectedQty", "The InspectedQty field is required");

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation Error before save.");

            if (svm.JobReceiveLineId <= 0)
                ModelState.AddModelError("JobReceiveLineId", "The JobReceiveLine field is required");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.JobReceiveQALineId <= 0)
                {

                    s.Sr = _JobReceiveQALineService.GetMaxSr(s.JobReceiveQAHeaderId);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                    }

                    new JobReceiveQAHeaderService(db).Update(temp, User.Identity.Name);

                    s.FailQty = s.QAQty - s.Qty;
                    s.FailDealQty = s.FailQty * s.UnitConversionMultiplier;

                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnQA(Mapper.Map<JobReceiveQALineViewModel>(s), temp.DocDate, ref db);

                    _JobReceiveQALineService.Create(s, User.Identity.Name);
                    //new JobReceiveQALineStatusService(_unitOfWork).CreateLineStatus(s.JobReceiveQALineId, ref db, true);

                    try
                    {
                        JobReceiveQADocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobReceiveQAHeaderId, s.JobReceiveQALineId, EventModeConstants.Add), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobReceiveQADocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobReceiveQAHeaderId, s.JobReceiveQALineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
                        DocLineId = s.JobReceiveQALineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.JobReceiveQAHeaderId, JobWorkerId = svm.JobWorkerId });
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    StringBuilder logstring = new StringBuilder();
                    JobReceiveQALine temp1 = _JobReceiveQALineService.Find(svm.JobReceiveQALineId);


                    JobReceiveQALine ExRec = new JobReceiveQALine();
                    ExRec = Mapper.Map<JobReceiveQALine>(temp1);

                    temp1.Remark = svm.Remark;
                    temp1.QAQty = svm.QAQty;
                    temp1.Qty = svm.Qty;
                    temp1.DealQty = svm.DealQty;
                    temp1.FailQty = temp1.QAQty - temp1.Qty;
                    temp1.FailDealQty = temp1.FailQty * temp1.UnitConversionMultiplier;
                    temp1.Weight = svm.Weight;
                    temp1.PenaltyAmt = svm.PenaltyAmt;
                    temp1.PenaltyRate = svm.PenaltyRate;
                    temp1.InspectedQty = svm.InspectedQty;
                    temp1.Marks = svm.Marks;

                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnQA(Mapper.Map<JobReceiveQALineViewModel>(temp1), temp.DocDate, ref db);

                    _JobReceiveQALineService.Update(temp1, User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                    }
                    new JobReceiveQAHeaderService(db).Update(temp, User.Identity.Name);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReceiveQADocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, temp1.JobReceiveQALineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobReceiveQADocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, temp1.JobReceiveQALineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
                        DocLineId = temp1.JobReceiveQALineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));


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
            JobReceiveQALineViewModel temp = _JobReceiveQALineService.GetJobReceiveQALine(id);

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

            JobReceiveQAHeader H = new JobReceiveQAHeaderService(db).Find(temp.JobReceiveQAHeaderId);

            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

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
            JobReceiveQALineViewModel temp = _JobReceiveQALineService.GetJobReceiveQALine(id);

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

            JobReceiveQAHeader H = new JobReceiveQAHeaderService(db).Find(temp.JobReceiveQAHeaderId);

            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            PrepareViewBag(temp);

            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobReceiveQALineViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveQADocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobReceiveQAHeaderId, vm.JobReceiveQALineId), ref db);
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


                JobReceiveQALine JobReceiveQALine = (from p in db.JobReceiveQALine
                                                     where p.JobReceiveQALineId == vm.JobReceiveQALineId
                                                     select p).FirstOrDefault();
                JobReceiveQALine.FailDealQty = 0;
                JobReceiveQALine.FailQty = 0;
                JobReceiveQALine.Weight = 0;
                JobReceiveQALine.PenaltyAmt = 0;

                JobReceiveQAHeader header = new JobReceiveQAHeaderService(db).Find(JobReceiveQALine.JobReceiveQAHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveQALine>(JobReceiveQALine),
                });



                new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnQA(Mapper.Map<JobReceiveQALineViewModel>(JobReceiveQALine), header.DocDate, ref db);

                IEnumerable<JobReceiveQAAttribute> AttributeList = (from L in db.JobReceiveQAAttribute where L.JobReceiveQALineId == JobReceiveQALine.JobReceiveQALineId select L).ToList();
                foreach (var Attribute in AttributeList)
                {
                    if (Attribute.JobReceiveQAAttributeId != null)
                    {
                        new JobReceiveQAAttributeService(_unitOfWork).Delete((int)Attribute.JobReceiveQAAttributeId);
                    }
                }

                IEnumerable<JobReceiveQAPenalty> PenaltyList = (from L in db.JobReceiveQAPenalty where L.JobReceiveQALineId == JobReceiveQALine.JobReceiveQALineId select L).ToList();
                foreach (var Penalty in PenaltyList)
                {
                    if (Penalty.JobReceiveQAPenaltyId != null)
                    {
                        new JobReceiveQAPenaltyService(db, _unitOfWork).Delete((int)Penalty.JobReceiveQAPenaltyId);
                    }
                }

                JobReceiveQALineExtended QALineExtended = (from L in db.JobReceiveQALineExtended where L.JobReceiveQALineId == JobReceiveQALine.JobReceiveQALineId select L).FirstOrDefault();
                if (QALineExtended != null)
                {
                    QALineExtended.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveQALineExtended.Remove(QALineExtended);
                }







                _JobReceiveQALineService.Delete(JobReceiveQALine);



                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;

                    new JobReceiveQAHeaderService(db).Update(header, User.Identity.Name);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobReceiveQADocEvents.onLineDeleteEvent(this, new JobEventArgs(JobReceiveQALine.JobReceiveQAHeaderId, JobReceiveQALine.JobReceiveQALineId), ref db);
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
                    JobReceiveQADocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobReceiveQALine.JobReceiveQAHeaderId, JobReceiveQALine.JobReceiveQALineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.JobReceiveQAHeaderId,
                    DocLineId = JobReceiveQALine.JobReceiveQALineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
        }

        public JsonResult GetBarCodesForProductUid(int[] Id)
        {
            return Json(_JobReceiveQALineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingReceive(int HeaderId, string term, int Limit)
        {
            return Json(_JobReceiveQALineService.GetPendingJobReceivesForAC(HeaderId, term, Limit).ToList());
        }

        public JsonResult GetProductUidValidation(string ProductUID, int HeaderID)
        {
            return Json(_JobReceiveQALineService.ValidateQABarCode(ProductUID, HeaderID), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetReceiveLineForUid(int UId, int HeaderId)
        {

            var JOLine = _JobReceiveQALineService.GetReceiveLineForUid(UId, HeaderId);
            if (JOLine == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(JOLine, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPendingJobReceiveProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _JobReceiveQALineService.GetPendingProductsForJobReceiveQA(searchTerm, filter);

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
        public JsonResult GetPendingJobReceives(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Query = _JobReceiveQALineService.GetPendingJobReceives(searchTerm, filter);

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

        public JsonResult GetReceiveDetail(int RecId, int ReceiveQAId)
        {
            return Json(_JobReceiveQALineService.GetLineDetailForQA(RecId, ReceiveQAId));
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
