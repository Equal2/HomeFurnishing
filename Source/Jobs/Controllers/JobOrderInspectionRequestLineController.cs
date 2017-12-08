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
using JobOrderInspectionRequestDocumentEvents;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderInspectionRequestLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;

        IJobOrderInspectionRequestLineService _JobOrderInspectionRequestLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public JobOrderInspectionRequestLineController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderInspectionRequestLineService = new JobOrderInspectionRequestLineService(db);
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
            var p = _JobOrderInspectionRequestLineService.GetJobOrderInspectionRequestLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            JobOrderInspectionRequestFilterViewModel vm = new JobOrderInspectionRequestFilterViewModel();
            vm.JobOrderInspectionRequestHeaderId = id;
            vm.JobWorkerId = sid;
            JobOrderInspectionRequestHeader Header = new JobOrderInspectionRequestHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderInspectionRequestFilterViewModel vm)
        {
            List<JobOrderInspectionRequestLineViewModel> temp = _JobOrderInspectionRequestLineService.GetJobOrderLineForMultiSelect(vm).ToList();
            JobOrderInspectionRequestMasterDetailModel svm = new JobOrderInspectionRequestMasterDetailModel();
            svm.JobOrderInspectionRequestViewModels = temp.Where(m => m.ProductUidId == null).ToList();
            JobOrderInspectionRequestHeader Header = new JobOrderInspectionRequestHeaderService(db).Find(vm.JobOrderInspectionRequestHeaderId);
            JobOrderInspectionRequestSettings settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);

            if (temp.Where(m => m.ProductUidId != null).Any())
            {
                List<JobOrderInspectionRequestLineViewModel> Sequence = new List<JobOrderInspectionRequestLineViewModel>();
                Sequence = temp.Where(m => m.ProductUidId != null).ToList();
                foreach (var item in Sequence)
                    item.JobInspectionType = JobReceiveTypeConstants.ProductUid;

                HttpContext.Session["UIQUID" + vm.JobOrderInspectionRequestHeaderId] = Sequence;
            }

            if (!temp.Where(m => m.ProductUidId == null).Any())
            {
                if (temp.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    List<BarCodeSequenceViewModelForInspection> Sequence = new List<BarCodeSequenceViewModelForInspection>();

                    var Grouping = (from p in temp.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobOrderHeaderId, p.ProductId, p.ProdOrderLineId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspection
                        {
                            JobOrderInspectionRequestHeaderId = item.Max(m => m.JobOrderInspectionRequestHeaderId),
                            //JobOrderLineId = item.JobOrderLineId,
                            JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionRequestType = JobReceiveTypeConstants.ProductUid,
                            JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                            // FirstBarCode = _JobOrderInspectionRequestLineService.GetFirstBarCodeForCancel(item.JobOrderLineId),
                        });

                    BarCodeSequenceListViewModelForInspection SquenceList = new BarCodeSequenceListViewModelForInspection();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionRequestHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

            }

            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderInspectionRequestMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobOrderInspectionRequestLineService.GetMaxSr(vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobOrderInspectionRequestLineViewModel> BarCodeBasedBranch = new List<JobOrderInspectionRequestLineViewModel>();

            BarCodeBasedBranch = (List<JobOrderInspectionRequestLineViewModel>)HttpContext.Session["UIQUID" + vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId];


            HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId);


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionRequestDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId), ref db);
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
                JobOrderInspectionRequestHeader Header = new JobOrderInspectionRequestHeaderService(db).Find(vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId);

                JobOrderInspectionRequestSettings Settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                var JobOrderLineIds = vm.JobOrderInspectionRequestViewModels.Select(m => m.JobOrderLineId).ToArray();

                var JobOrderLineBalanceQtyRecords = (from p in db.ViewJobOrderBalanceForInspectionRequest
                                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                                     select new { BalQty = p.BalanceQty, LineId = p.JobOrderLineId }).ToList();

                var JobORderLineRecords = (from p in db.JobOrderLine
                                           where JobOrderLineIds.Contains(p.JobOrderLineId)
                                           select p).ToList();

                var JobOrderHeaderIds = JobORderLineRecords.Select(m => m.JobOrderHeaderId).ToArray();

                var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                             where JobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                             select p).ToList();

                var ProductUids = JobORderLineRecords.Select(m => m.ProductUidId).ToArray();

                var ProductUidsRecords = (from p in db.ProductUid
                                          where ProductUids.Contains(p.ProductUIDId)
                                          select p).ToList();

                foreach (var item in vm.JobOrderInspectionRequestViewModels.Where(m => m.Qty > 0))
                {

                    var balqty = JobOrderLineBalanceQtyRecords.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().BalQty;

                    JobOrderLine JobOrderLine = JobORderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                    JobOrderHeader JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();

                    if (item.Qty > 0 && item.Qty <= balqty && JobOrderLine.ProductUidId == null)
                    {

                        JobOrderInspectionRequestLine line = new JobOrderInspectionRequestLine();
                        line.JobOrderInspectionRequestLineId = -Cnt;
                        line.JobOrderInspectionRequestHeaderId = item.JobOrderInspectionRequestHeaderId;
                        line.JobOrderLineId = item.JobOrderLineId;
                        line.ProductUidId = JobOrderLine.ProductUidId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        LineStatus.Add(line.JobOrderLineId, line.Qty);



                        line.ObjectState = Model.ObjectState.Added;
                        db.JobOrderInspectionRequestLine.Add(line);
                        //_JobOrderInspectionRequestLineService.Create(line);

                        //new JobOrderInspectionRequestLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderInspectionRequestLineId, ref db, true);

                        Cnt = Cnt + 1;

                    }
                }

                //new JobOrderInspectionRequestHeaderService(_unitOfWork).Update(Header);
                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderInspectionRequestHeader.Add(Header);
                //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionRequestDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId), ref db);
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
                    JobOrderInspectionRequestDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }               

                if (BarCodeBasedBranch != null && BarCodeBasedBranch.Count() > 0)
                {
                    List<BarCodeSequenceViewModelForInspection> Sequence = new List<BarCodeSequenceViewModelForInspection>();

                    var Grouping = (from p in BarCodeBasedBranch.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobOrderHeaderId, p.ProductId, p.ProdOrderLineId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspection
                        {
                            JobOrderInspectionRequestHeaderId = item.Max(m => m.JobOrderInspectionRequestHeaderId),
                            //JobOrderLineId = item.JobOrderLineId,
                            JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionRequestType = JobReceiveTypeConstants.ProductUid,
                            JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                            // FirstBarCode = _JobOrderInspectionRequestLineService.GetFirstBarCodeForCancel(item.JobOrderLineId),
                        });

                    BarCodeSequenceListViewModelForInspection SquenceList = new BarCodeSequenceListViewModelForInspection();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionRequestViewModels.FirstOrDefault().JobOrderInspectionRequestHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionRequestHeaderId,
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
        public ActionResult _SequencePost2(BarCodeSequenceListViewModelForInspection vm)
        {
            List<BarCodeSequenceViewModelForInspection> Seq = new List<BarCodeSequenceViewModelForInspection>();
            BarCodeSequenceListViewModelForInspection SquLis = new BarCodeSequenceListViewModelForInspection();

            JobOrderInspectionRequestHeader Header = new JobOrderInspectionRequestHeaderService(db).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderInspectionRequestHeaderId);

            var LineIds = vm.BarCodeSequenceViewModel.Select(m => m.JobOrderLineId).ToArray();


            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                if (item.JobOrderInspectionRequestType == JobReceiveTypeConstants.ProductUid)
                {
                    string jOLIDs = item.JobOrdLineIds;

                    BarCodeSequenceViewModelForInspection SeqLi = new BarCodeSequenceViewModelForInspection();

                    SeqLi.JobOrderInspectionRequestHeaderId = item.JobOrderInspectionRequestHeaderId;
                    SeqLi.JobOrdLineIds = item.JobOrdLineIds;
                    //SequenceLine.TJOLID = item.JobOrderLineId;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobOrderInspectionRequestType = item.JobOrderInspectionRequestType;
                    SeqLi.JobOrderHeaderId = item.JobOrderHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobOrderInspectionRequestLineService.GetPendingBarCodesList(SeqLi.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobOrderInspectionRequestLineService.GetPendingBarCodesList(SeqLi.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray());

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
        public ActionResult _BarCodesPost(BarCodeSequenceListViewModelForInspection vm)
        {

            int Cnt = 0;
            int Pk = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            int Serial = _JobOrderInspectionRequestLineService.GetMaxSr(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionRequestDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            var JobOrderLineIds = vm.BarCodeSequenceViewModelPost.Select(m => m.JobOrderLineId).ToArray();

            var JobOrderLineRecords = (from p in db.JobOrderLine
                                       where JobOrderLineIds.Contains(p.JobOrderLineId)
                                       select p).ToList();

            var ProductUidJobOrderLineIds = string.Join(",", vm.BarCodeSequenceViewModelPost.Where(m => !string.IsNullOrEmpty(m.JobOrdLineIds)).Select(m => m.JobOrdLineIds));

            int[] ProdUidJobOrderLineIds;

            ProdUidJobOrderLineIds = string.IsNullOrEmpty(ProductUidJobOrderLineIds) ? new int[0] : ProductUidJobOrderLineIds.Split(',').Select(Int32.Parse).ToArray();

            var ProdUidJobOrderLineRecords = (from p in db.ViewJobOrderBalanceForInspectionRequest
                                              join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                              where ProdUidJobOrderLineIds.Contains(p.JobOrderLineId)
                                              select t).ToList();
            var ProdUidJobOrderHeaderIds = ProdUidJobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var ProdUidJobOrderHeaderRecords = (from p in db.JobOrderHeader
                                                where ProdUidJobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                                select p).ToList();

            var JobOrderHeaderIds = JobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                         where JobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                         select p).ToList();

            var SProductUids = string.Join(",", vm.BarCodeSequenceViewModelPost.Select(m => m.ProductUidIds));

            var ProductUids = SProductUids.Split(',').Select(Int32.Parse).ToArray();

            var ProductUidRecords = (from p in db.ProductUid
                                     where ProductUids.Contains(p.ProductUIDId)
                                     select p).ToList();

            var BalanceQtyRecords = (from p in db.ViewJobOrderBalanceForInspectionRequest
                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                     select new { BalQty = p.BalanceQty, JobOrderLineId = p.JobOrderLineId }).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobOrderInspectionRequestHeader Header = new JobOrderInspectionRequestHeaderService(db).Find(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestHeaderId);
                JobOrderInspectionRequestSettings Settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);


                foreach (var item in vm.BarCodeSequenceViewModelPost)
                {

                    //JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(item.JobOrderLineId);
                    //JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);
                    JobOrderLine JobOrderLine = new JobOrderLine();
                    JobOrderHeader JobOrderHeader = new JobOrderHeader();

                    //JobOrderLine JobOrderLine = JobOrderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                    // JobOrderHeader JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();                  

                    decimal balqty = 0;

                    if (item.JobOrderInspectionRequestType == JobReceiveTypeConstants.ProductUid)
                    {
                        var LineIds = item.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray();
                        balqty = (from L in ProdUidJobOrderLineRecords
                                  where LineIds.Contains(L.JobOrderLineId)
                                  select L.Qty).Sum();
                    }


                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {
                            if (item.JobOrderInspectionRequestType == JobReceiveTypeConstants.ProductUid)
                            {
                                JobOrderLine = ProdUidJobOrderLineRecords.Where(m => m.ProductUidId == BarCodes && m.JobOrderHeaderId == item.JobOrderHeaderId).FirstOrDefault();
                                JobOrderHeader = ProdUidJobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();
                            }

                            JobOrderInspectionRequestLine line = new JobOrderInspectionRequestLine();

                            line.JobOrderInspectionRequestHeaderId = item.JobOrderInspectionRequestHeaderId;

                            if (item.JobOrderInspectionRequestType == JobReceiveTypeConstants.ProductUid)
                            {
                                line.JobOrderLineId = JobOrderLine.JobOrderLineId;
                            }
                            line.ProductUidId = BarCodes;
                            line.Qty = 1;
                            line.Sr = Serial++;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobOrderInspectionRequestLineId = Pk++;

                            if (LineStatus.ContainsKey(line.JobOrderLineId))
                            {
                                LineStatus[line.JobOrderLineId] = LineStatus[line.JobOrderLineId] + 1;
                            }
                            else
                            {
                                LineStatus.Add(line.JobOrderLineId, line.Qty);
                            }


                            line.ObjectState = Model.ObjectState.Added;
                            db.JobOrderInspectionRequestLine.Add(line);

                            //new JobOrderInspectionRequestLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderInspectionRequestLineId, ref db, true);


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
                db.JobOrderInspectionRequestHeader.Add(Header);
                //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionRequestDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestHeaderId), ref db);
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
                    JobOrderInspectionRequestDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionRequestHeaderId,
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
            JobOrderInspectionRequestHeader header = new JobOrderInspectionRequestHeaderService(db).Find(Id);
            JobOrderInspectionRequestLineViewModel svm = new JobOrderInspectionRequestLineViewModel();

            //Getting Settings
            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
            ViewBag.LineMode = "Create";
            svm.JobOrderInspectionRequestHeaderId = Id;
            svm.JobWorkerId = sid;

            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }

            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobOrderInspectionRequestLineViewModel svm)
        {

            List<JobOrderInspectionRequestLine> RequestLines = new List<JobOrderInspectionRequestLine>();
            bool BeforeSave = true;

            try
            {
                if (svm.JobOrderInspectionRequestLineId <= 0)
                    BeforeSave = JobOrderInspectionRequestDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderInspectionRequestHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobOrderInspectionRequestDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderInspectionRequestHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (svm.JobOrderInspectionRequestLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.JobOrderInspectionRequestLineId <= 0 && BeforeSave && !EventException)
            {
                JobOrderInspectionRequestHeader temp = new JobOrderInspectionRequestHeaderService(db).Find(svm.JobOrderInspectionRequestHeaderId);


                decimal balqty = (from p in db.ViewJobOrderBalanceForInspectionRequest
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
                if (svm.JobOrderLineId == 0)
                {
                    ModelState.AddModelError("JobOrderLineId", "JobOrder field is required.");
                }
                if (ModelState.IsValid)
                {
                    int pk = 0;
                    int Sr = _JobOrderInspectionRequestLineService.GetMaxSr(svm.JobOrderInspectionRequestHeaderId);
                    JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(svm.JobOrderLineId);
                    JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);


                    JobOrderInspectionRequestLine RequestLine = new JobOrderInspectionRequestLine();
                    RequestLine.Remark = svm.Remark;
                    RequestLine.JobOrderInspectionRequestHeaderId = svm.JobOrderInspectionRequestHeaderId;
                    RequestLine.JobOrderLineId = svm.JobOrderLineId;
                    RequestLine.Qty = svm.Qty;
                    RequestLine.ProductUidId = svm.ProductUidId;
                    RequestLine.Sr = Sr++;
                    RequestLine.CreatedDate = DateTime.Now;
                    RequestLine.ModifiedDate = DateTime.Now;
                    RequestLine.CreatedBy = User.Identity.Name;
                    RequestLine.ModifiedBy = User.Identity.Name;
                    RequestLine.JobOrderInspectionRequestLineId = pk++;


                    //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnRequest(svm.JobOrderLineId, svm.JobOrderInspectionRequestLineId, temp.DocDate, svm.Qty, ref db, true);
                    

                    RequestLine.ObjectState = Model.ObjectState.Added;
                    db.JobOrderInspectionRequestLine.Add(RequestLine);


                    //JobOrderInspectionRequestHeader temp2 = new JobOrderInspectionRequestHeaderService(_unitOfWork).Find(s.JobOrderInspectionRequestHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    { temp.Status = (int)StatusConstants.Modified;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ModifiedDate = DateTime.Now;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestHeader.Add(temp);
                    //new JobOrderInspectionRequestHeaderService(_unitOfWork).Update(temp);

                    try
                    {
                        JobOrderInspectionRequestDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestHeaderId, EventModeConstants.Add), ref db);
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
                        JobOrderInspectionRequestDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionRequestHeaderId,
                        DocLineId = RequestLine.JobOrderInspectionRequestLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.JobOrderInspectionRequestHeaderId, sid = svm.JobWorkerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                JobOrderInspectionRequestHeader temp = new JobOrderInspectionRequestHeaderService(db).Find(svm.JobOrderInspectionRequestHeaderId);
                int status = temp.Status;

                JobOrderInspectionRequestLine s = _JobOrderInspectionRequestLineService.Find(svm.JobOrderInspectionRequestLineId);


                JobOrderInspectionRequestLine ExRec = new JobOrderInspectionRequestLine();
                ExRec = Mapper.Map<JobOrderInspectionRequestLine>(s);


                decimal balqty = (from p in db.ViewJobOrderBalanceForInspectionRequest
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
                        //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnRequest(s.JobOrderLineId, s.JobOrderInspectionRequestLineId, temp.DocDate, s.Qty, ref db, true);

                    }

                    //_JobOrderInspectionRequestLineService.Update(s);
                    s.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestLine.Add(s);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = s,
                    });

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    { temp.Status = (int)StatusConstants.Modified;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    }

                    //new JobOrderInspectionRequestHeaderService(_unitOfWork).Update(temp);

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestHeader.Add(temp);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobOrderInspectionRequestDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestHeaderId, s.JobOrderInspectionRequestLineId, EventModeConstants.Edit), ref db);
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
                        JobOrderInspectionRequestDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestHeaderId, s.JobOrderInspectionRequestLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionRequestHeaderId,
                        DocLineId = s.JobOrderInspectionRequestLineId,
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
            JobOrderInspectionRequestLineViewModel temp = _JobOrderInspectionRequestLineService.GetJobOrderInspectionRequestLine(id);

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

            JobOrderInspectionRequestHeader header = new JobOrderInspectionRequestHeaderService(db).Find(temp.JobOrderInspectionRequestHeaderId);

            //Getting Settings
            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            var JobOrderLine = new JobOrderLineService(_unitOfWork).Find(temp.JobOrderLineId);
            if (temp.ProductUidId.HasValue)
            {
                ViewBag.BarCodeGenerated = true;
                temp.BarCodes = temp.ProductUidId.ToString();
            }
            temp.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);
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
            JobOrderInspectionRequestLineViewModel temp = _JobOrderInspectionRequestLineService.GetJobOrderInspectionRequestLine(id);

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

            JobOrderInspectionRequestHeader header = new JobOrderInspectionRequestHeaderService(db).Find(temp.JobOrderInspectionRequestHeaderId);

            //Getting Settings
            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            var JobOrderLine = new JobOrderLineService(_unitOfWork).Find(temp.JobOrderLineId);

            if (temp.ProductUidId.HasValue)
            {
                ViewBag.BarCodeGenerated = true;
                temp.BarCodes = temp.ProductUidId.ToString();
            }
            temp.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);

            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobOrderInspectionRequestLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionRequestDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestHeaderId, vm.JobOrderInspectionRequestLineId), ref db);
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

                JobOrderInspectionRequestLine JobOrderInspectionRequestLine = (from p in db.JobOrderInspectionRequestLine
                                                                               where p.JobOrderInspectionRequestLineId == vm.JobOrderInspectionRequestLineId
                                                                               select p).FirstOrDefault();

                JobOrderInspectionRequestHeader RequestHEader = new JobOrderInspectionRequestHeaderService(db).Find(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobOrderInspectionRequestLine>(JobOrderInspectionRequestLine),
                });

                //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnRequest(JobOrderInspectionRequestLine.JobOrderLineId, JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId, RequestHEader.DocDate, 0, ref db, true);               


                //_JobOrderInspectionRequestLineService.Delete(vm.JobOrderInspectionRequestLineId);               

                JobOrderInspectionRequestLine.ObjectState = Model.ObjectState.Deleted;
                db.JobOrderInspectionRequestLine.Remove(JobOrderInspectionRequestLine);

                JobOrderInspectionRequestHeader header = new JobOrderInspectionRequestHeaderService(db).Find(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId);
                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new JobOrderInspectionRequestHeaderService(_unitOfWork).Update(header);
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestHeader.Add(header);
                }


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobOrderInspectionRequestDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId, JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId), ref db);
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
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    JobOrderInspectionRequestDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId, JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = RequestHEader.DocTypeId,
                    DocId = RequestHEader.JobOrderInspectionRequestHeaderId,
                    DocLineId = JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                   
                    DocNo = RequestHEader.DocNo,
                    xEModifications = Modifications,
                    DocDate = RequestHEader.DocDate,
                    DocStatus=RequestHEader.Status,
                }));

            }
            return Json(new { success = true });
        }

        public JsonResult GetProductUidValidation(string ProductUID, int HeaderID)
        {
            return Json(_JobOrderInspectionRequestLineService.ValidateInspectionRequestBarCode(ProductUID, HeaderID), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(_JobOrderInspectionRequestLineService.GetLineDetailForInsReq(LineId));
        }
        public JsonResult GetOrderLineForUid(int UId)
        {

            var JOLine = _JobOrderInspectionRequestLineService.GetOrderLineForUidBranch(UId);
            if (JOLine == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(JOLine, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingOrders(int HeaderId, string term, int Limit)
        {
            return Json(_JobOrderInspectionRequestLineService.GetPendingJobOrdersForAC(HeaderId, term, Limit).ToList());
        }

        public JsonResult CheckDuplicateJobOrder(int LineId, int RequestHeaderId)
        {
            return Json(_JobOrderInspectionRequestLineService.CheckForDuplicateJobOrder(LineId, RequestHeaderId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBarCodesForProductUid(int[] Id)
        {
            return Json(_JobOrderInspectionRequestLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPendingJobOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionRequestLineService.GetPendingProductHelpList(searchTerm, filter);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
        public JsonResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionRequestLineService.GetPendingJobOrders(searchTerm, filter);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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
