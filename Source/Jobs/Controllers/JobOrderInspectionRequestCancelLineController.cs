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
using JobOrderInspectionRequestCancelDocumentEvents;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderInspectionRequestCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;
        IJobOrderInspectionRequestCancelLineService _JobOrderInspectionRequestCancelLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public JobOrderInspectionRequestCancelLineController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderInspectionRequestCancelLineService = new JobOrderInspectionRequestCancelLineService(db);
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
            var p = _JobOrderInspectionRequestCancelLineService.GetJobOrderInspectionRequestCancelLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            JobOrderInspectionRequestCancelFilterViewModel vm = new JobOrderInspectionRequestCancelFilterViewModel();
            vm.JobOrderInspectionRequestCancelHeaderId = id;
            vm.JobWorkerId = sid;
            JobOrderInspectionRequestCancelHeader Header = new JobOrderInspectionRequestCancelHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderInspectionRequestCancelFilterViewModel vm)
        {
            List<JobOrderInspectionRequestCancelLineViewModel> temp = _JobOrderInspectionRequestCancelLineService.GetJobOrderInspectionRequestLineForMultiSelect(vm).ToList();
            JobOrderInspectionRequestCancelMasterDetailModel svm = new JobOrderInspectionRequestCancelMasterDetailModel();
            svm.JobOrderInspectionRequestCancelViewModels = temp.Where(m => m.ProductUidId == null).ToList();
            JobOrderInspectionRequestCancelHeader Header = new JobOrderInspectionRequestCancelHeaderService(db).Find(vm.JobOrderInspectionRequestCancelHeaderId);
            JobOrderInspectionRequestSettings settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);

            if (temp.Where(m => m.ProductUidId != null).Any())
            {
                List<JobOrderInspectionRequestCancelLineViewModel> Sequence = new List<JobOrderInspectionRequestCancelLineViewModel>();
                Sequence = temp.Where(m => m.ProductUidId != null).ToList();
                foreach (var item in Sequence)
                    item.JobInspectionType = JobReceiveTypeConstants.ProductUid;

                HttpContext.Session["UIQUID" + vm.JobOrderInspectionRequestCancelHeaderId] = Sequence;
            }

            if (!temp.Where(m => m.ProductUidId == null).Any())
            {
                if (temp.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    List<BarCodeSequenceViewModelForInspectionRequestCancel> Sequence = new List<BarCodeSequenceViewModelForInspectionRequestCancel>();

                    var Grouping = (from p in temp.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobOrderInspectionRequestHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspectionRequestCancel
                        {
                            JobOrderInspectionRequestCancelHeaderId = item.Max(m => m.JobOrderInspectionRequestCancelHeaderId),
                            //JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId,
                            JobOrdInspectionRequestLineIds = string.Join(",", item.Select(m => m.JobOrderInspectionRequestLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionRequestCancelType = JobReceiveTypeConstants.ProductUid,
                            JobOrderInspectionRequestHeaderId = item.Max(m => m.JobOrderInspectionRequestHeaderId),
                            // FirstBarCode = _JobOrderInspectionRequestCancelLineService.GetFirstBarCodeForCancel(item.JobOrderInspectionRequestLineId),
                        });

                    BarCodeSequenceListViewModelForInspectionRequestCancel SquenceList = new BarCodeSequenceListViewModelForInspectionRequestCancel();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionRequestCancelHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

            }

            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderInspectionRequestCancelMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobOrderInspectionRequestCancelLineService.GetMaxSr(vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobOrderInspectionRequestCancelLineViewModel> BarCodeBasedBranch = new List<JobOrderInspectionRequestCancelLineViewModel>();

            BarCodeBasedBranch = (List<JobOrderInspectionRequestCancelLineViewModel>)HttpContext.Session["UIQUID" + vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId];


            HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionRequestCancelDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId), ref db);
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
                JobOrderInspectionRequestCancelHeader Header = new JobOrderInspectionRequestCancelHeaderService(db).Find(vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);

                JobOrderInspectionRequestSettings Settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                var JobOrderInspectionRequestLineIds = vm.JobOrderInspectionRequestCancelViewModels.Select(m => m.JobOrderInspectionRequestLineId).ToArray();


                var JobOrderInspectionRequestLineBalanceQtyRecords = (from p in db.ViewJobOrderInspectionRequestBalance
                                                                      where JobOrderInspectionRequestLineIds.Contains(p.JobOrderInspectionRequestLineId)
                                                                      select new { BalQty = p.BalanceQty, LineId = p.JobOrderInspectionRequestLineId }).ToList();

                var JobOrderInspectionRequestLineRecords = (from p in db.JobOrderInspectionRequestLine
                                                            where JobOrderInspectionRequestLineIds.Contains(p.JobOrderInspectionRequestLineId)
                                                            select p).ToList();

                var JobOrderInspectionRequestHeaderIds = JobOrderInspectionRequestLineRecords.Select(m => m.JobOrderInspectionRequestHeaderId).ToArray();

                var JobOrderInspectionRequestHeaderRecords = (from p in db.JobOrderInspectionRequestHeader
                                                              where JobOrderInspectionRequestHeaderIds.Contains(p.JobOrderInspectionRequestHeaderId)
                                                              select p).ToList();

                var ProductUids = JobOrderInspectionRequestLineRecords.Select(m => m.ProductUidId).ToArray();

                var ProductUidsRecords = (from p in db.ProductUid
                                          where ProductUids.Contains(p.ProductUIDId)
                                          select p).ToList();

                foreach (var item in vm.JobOrderInspectionRequestCancelViewModels.Where(m => m.Qty > 0))
                {
                    var balqty = JobOrderInspectionRequestLineBalanceQtyRecords.Where(m => m.LineId == item.JobOrderInspectionRequestLineId).FirstOrDefault().BalQty;

                    JobOrderInspectionRequestLine JobOrderInspectionRequestLine = JobOrderInspectionRequestLineRecords.Where(m => m.JobOrderInspectionRequestLineId == item.JobOrderInspectionRequestLineId).FirstOrDefault();
                    JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = JobOrderInspectionRequestHeaderRecords.Where(m => m.JobOrderInspectionRequestHeaderId == JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty && JobOrderInspectionRequestLine.ProductUidId == null)
                    {

                        JobOrderInspectionRequestCancelLine line = new JobOrderInspectionRequestCancelLine();
                        line.JobOrderInspectionRequestCancelLineId = -Cnt;
                        line.JobOrderInspectionRequestCancelHeaderId = item.JobOrderInspectionRequestCancelHeaderId;
                        line.JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId;
                        line.ProductUidId = JobOrderInspectionRequestLine.ProductUidId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.Remark = item.Remark;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        LineStatus.Add(line.JobOrderInspectionRequestLineId, line.Qty);



                        line.ObjectState = Model.ObjectState.Added;
                        db.JobOrderInspectionRequestCancelLine.Add(line);
                        //_JobOrderInspectionRequestCancelLineService.Create(line);

                        //new JobOrderInspectionRequestCancelLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderInspectionRequestCancelLineId, ref db, true);

                        Cnt = Cnt + 1;

                    }
                }

                //new JobOrderInspectionRequestCancelHeaderService(_unitOfWork).Update(Header);
                if (Header.Status != (int)StatusConstants.Drafted && Header.Status!=(int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderInspectionRequestCancelHeader.Add(Header);
                //new JobOrderInspectionRequestLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionRequestCancelDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId), ref db);
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
                    JobOrderInspectionRequestCancelDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }               


                if (BarCodeBasedBranch != null && BarCodeBasedBranch.Count() > 0)
                {
                    List<BarCodeSequenceViewModelForInspectionRequestCancel> Sequence = new List<BarCodeSequenceViewModelForInspectionRequestCancel>();

                    var Grouping = (from p in BarCodeBasedBranch.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobOrderInspectionRequestHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspectionRequestCancel
                        {
                            JobOrderInspectionRequestCancelHeaderId = item.Max(m => m.JobOrderInspectionRequestCancelHeaderId),
                            //JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId,
                            JobOrdInspectionRequestLineIds = string.Join(",", item.Select(m => m.JobOrderInspectionRequestLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionRequestCancelType = JobReceiveTypeConstants.ProductUid,
                            JobOrderInspectionRequestHeaderId = item.Max(m => m.JobOrderInspectionRequestHeaderId),
                            // FirstBarCode = _JobOrderInspectionRequestCancelLineService.GetFirstBarCodeForCancel(item.JobOrderInspectionRequestLineId),
                        });

                    BarCodeSequenceListViewModelForInspectionRequestCancel SquenceList = new BarCodeSequenceListViewModelForInspectionRequestCancel();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionRequestCancelViewModels.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionRequestCancelHeaderId,
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
        public ActionResult _SequencePost2(BarCodeSequenceListViewModelForInspectionRequestCancel vm)
        {
            List<BarCodeSequenceViewModelForInspectionRequestCancel> Seq = new List<BarCodeSequenceViewModelForInspectionRequestCancel>();
            BarCodeSequenceListViewModelForInspectionRequestCancel SquLis = new BarCodeSequenceListViewModelForInspectionRequestCancel();

            JobOrderInspectionRequestCancelHeader Header = new JobOrderInspectionRequestCancelHeaderService(db).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);


            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                if (item.JobOrderInspectionRequestCancelType == JobReceiveTypeConstants.ProductUid)
                {
                    string jOLIDs = item.JobOrdInspectionRequestLineIds;

                    BarCodeSequenceViewModelForInspectionRequestCancel SeqLi = new BarCodeSequenceViewModelForInspectionRequestCancel();

                    SeqLi.JobOrderInspectionRequestCancelHeaderId = item.JobOrderInspectionRequestCancelHeaderId;
                    SeqLi.JobOrdInspectionRequestLineIds = item.JobOrdInspectionRequestLineIds;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobOrderInspectionRequestCancelType = item.JobOrderInspectionRequestCancelType;
                    SeqLi.JobOrderInspectionRequestHeaderId = item.JobOrderInspectionRequestHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobOrderInspectionRequestCancelLineService.GetPendingBarCodesList(SeqLi.JobOrdInspectionRequestLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobOrderInspectionRequestCancelLineService.GetPendingBarCodesList(SeqLi.JobOrdInspectionRequestLineIds.Split(',').Select(Int32.Parse).ToArray());

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
        public ActionResult _BarCodesPost(BarCodeSequenceListViewModelForInspectionRequestCancel vm)
        {

            int Cnt = 0;
            int Pk = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            int Serial = _JobOrderInspectionRequestCancelLineService.GetMaxSr(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionRequestCancelDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            var ProductUidJobOrderInspectionRequestLineIds = string.Join(",", vm.BarCodeSequenceViewModelPost.Where(m => !string.IsNullOrEmpty(m.JobOrdInspectionRequestLineIds)).Select(m => m.JobOrdInspectionRequestLineIds));

            int[] ProdUidJobOrderInspectionRequestLineIds;

            ProdUidJobOrderInspectionRequestLineIds = string.IsNullOrEmpty(ProductUidJobOrderInspectionRequestLineIds) ? new int[0] : ProductUidJobOrderInspectionRequestLineIds.Split(',').Select(Int32.Parse).ToArray();

            var ProdUidJobOrderInspectionRequestLineRecords = (from p in db.ViewJobOrderInspectionRequestBalance
                                                               join t in db.JobOrderInspectionRequestLine on p.JobOrderInspectionRequestLineId equals t.JobOrderInspectionRequestLineId
                                                               where ProdUidJobOrderInspectionRequestLineIds.Contains(p.JobOrderInspectionRequestLineId)
                                                               select t).ToList();
            var ProdUidJobOrderInspectionRequestHeaderIds = ProdUidJobOrderInspectionRequestLineRecords.GroupBy(m => m.JobOrderInspectionRequestHeaderId).Select(m => m.Key).ToArray();

            var ProdUidJobOrderInspectionRequestHeaderRecords = (from p in db.JobOrderInspectionRequestHeader
                                                                 where ProdUidJobOrderInspectionRequestHeaderIds.Contains(p.JobOrderInspectionRequestHeaderId)
                                                                 select p).ToList();

            var SProductUids = string.Join(",", vm.BarCodeSequenceViewModelPost.Select(m => m.ProductUidIds));

            var ProductUids = SProductUids.Split(',').Select(Int32.Parse).ToArray();

            var ProductUidRecords = (from p in db.ProductUid
                                     where ProductUids.Contains(p.ProductUIDId)
                                     select p).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobOrderInspectionRequestCancelHeader Header = new JobOrderInspectionRequestCancelHeaderService(db).Find(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId);

                foreach (var item in vm.BarCodeSequenceViewModelPost)
                {

                    //JobOrderInspectionRequestLine JobOrderInspectionRequestLine = new JobOrderInspectionRequestLineService(_unitOfWork).Find(item.JobOrderInspectionRequestLineId);
                    //JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = new JobOrderInspectionRequestHeaderService(_unitOfWork).Find(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId);
                    JobOrderInspectionRequestLine JobOrderInspectionRequestLine = new JobOrderInspectionRequestLine();
                    JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = new JobOrderInspectionRequestHeader();

                    //JobOrderInspectionRequestLine JobOrderInspectionRequestLine = JobOrderInspectionRequestLineRecords.Where(m => m.JobOrderInspectionRequestLineId == item.JobOrderInspectionRequestLineId).FirstOrDefault();
                    // JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = JobOrderInspectionRequestHeaderRecords.Where(m => m.JobOrderInspectionRequestHeaderId == JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId).FirstOrDefault();                  

                    decimal balqty = 0;

                    if (item.JobOrderInspectionRequestCancelType == JobReceiveTypeConstants.ProductUid)
                    {
                        var LineIds = item.JobOrdInspectionRequestLineIds.Split(',').Select(Int32.Parse).ToArray();
                        balqty = (from L in ProdUidJobOrderInspectionRequestLineRecords
                                  where LineIds.Contains(L.JobOrderInspectionRequestLineId)
                                  select L.Qty).Sum();
                    }


                    if (!string.IsNullOrEmpty(item.ProductUidIds) && item.ProductUidIds.Split(',').Count() <= balqty)
                    {

                        foreach (var BarCodes in item.ProductUidIds.Split(',').Select(Int32.Parse).ToList())
                        {
                            if (item.JobOrderInspectionRequestCancelType == JobReceiveTypeConstants.ProductUid)
                            {
                                JobOrderInspectionRequestLine = ProdUidJobOrderInspectionRequestLineRecords.Where(m => m.ProductUidId == BarCodes && m.JobOrderInspectionRequestHeaderId == item.JobOrderInspectionRequestHeaderId).FirstOrDefault();
                                JobOrderInspectionRequestHeader = ProdUidJobOrderInspectionRequestHeaderRecords.Where(m => m.JobOrderInspectionRequestHeaderId == JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId).FirstOrDefault();
                            }

                            JobOrderInspectionRequestCancelLine line = new JobOrderInspectionRequestCancelLine();

                            line.JobOrderInspectionRequestCancelHeaderId = item.JobOrderInspectionRequestCancelHeaderId;

                            if (item.JobOrderInspectionRequestCancelType == JobReceiveTypeConstants.ProductUid)
                            {
                                line.JobOrderInspectionRequestLineId = JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId;
                            }
                            line.ProductUidId = BarCodes;
                            line.Qty = 1;
                            line.Sr = Serial++;                           
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobOrderInspectionRequestCancelLineId = Pk++;

                            if (LineStatus.ContainsKey(line.JobOrderInspectionRequestLineId))
                            {
                                LineStatus[line.JobOrderInspectionRequestLineId] = LineStatus[line.JobOrderInspectionRequestLineId] + 1;
                            }
                            else
                            {
                                LineStatus.Add(line.JobOrderInspectionRequestLineId, line.Qty);
                            }


                            line.ObjectState = Model.ObjectState.Added;
                            db.JobOrderInspectionRequestCancelLine.Add(line);

                            //new JobOrderInspectionRequestCancelLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderInspectionRequestCancelLineId, ref db, true);


                            Cnt = Cnt + 1;
                        }
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status!=(int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderInspectionRequestCancelHeader.Add(Header);
                //new JobOrderInspectionRequestLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionRequestCancelDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId), ref db);
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
                    JobOrderInspectionRequestCancelDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionRequestCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionRequestCancelHeaderId,
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
            JobOrderInspectionRequestCancelHeader header = new JobOrderInspectionRequestCancelHeaderService(db).Find(Id);
            JobOrderInspectionRequestCancelLineViewModel svm = new JobOrderInspectionRequestCancelLineViewModel();

            //Getting Settings
            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
            ViewBag.LineMode = "Create";
            svm.JobOrderInspectionRequestCancelHeaderId = Id;
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
        public ActionResult _CreatePost(JobOrderInspectionRequestCancelLineViewModel svm)
        {

            List<JobOrderInspectionRequestCancelLine> RequestLines = new List<JobOrderInspectionRequestCancelLine>();
            bool BeforeSave = true;

            try
            {
                if (svm.JobOrderInspectionRequestCancelLineId <= 0)
                    BeforeSave = JobOrderInspectionRequestCancelDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderInspectionRequestCancelHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobOrderInspectionRequestCancelDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderInspectionRequestCancelHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (svm.JobOrderInspectionRequestCancelLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.JobOrderInspectionRequestCancelLineId <= 0 && BeforeSave && !EventException)
            {
                JobOrderInspectionRequestCancelHeader temp = new JobOrderInspectionRequestCancelHeaderService(db).Find(svm.JobOrderInspectionRequestCancelHeaderId);


                decimal balqty = (from p in db.ViewJobOrderInspectionRequestBalance
                                  where p.JobOrderInspectionRequestLineId == svm.JobOrderInspectionRequestLineId
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
                    int Sr = _JobOrderInspectionRequestCancelLineService.GetMaxSr(svm.JobOrderInspectionRequestCancelHeaderId);
                    JobOrderInspectionRequestLine JobOrderInspectionRequestLine = new JobOrderInspectionRequestLineService(db).Find(svm.JobOrderInspectionRequestLineId);
                    JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = new JobOrderInspectionRequestHeaderService(db).Find(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId);


                    JobOrderInspectionRequestCancelLine RequestLine = new JobOrderInspectionRequestCancelLine();
                    RequestLine.Remark = svm.Remark;
                    RequestLine.JobOrderInspectionRequestCancelHeaderId = svm.JobOrderInspectionRequestCancelHeaderId;
                    RequestLine.JobOrderInspectionRequestLineId = svm.JobOrderInspectionRequestLineId;
                    RequestLine.Qty = svm.Qty;
                    RequestLine.ProductUidId = svm.ProductUidId;
                    RequestLine.Sr = Sr++;
                    RequestLine.CreatedDate = DateTime.Now;
                    RequestLine.ModifiedDate = DateTime.Now;
                    RequestLine.CreatedBy = User.Identity.Name;
                    RequestLine.ModifiedBy = User.Identity.Name;
                    RequestLine.JobOrderInspectionRequestCancelLineId = pk++;


                    RequestLine.ObjectState = Model.ObjectState.Added;
                    db.JobOrderInspectionRequestCancelLine.Add(RequestLine);


                    //JobOrderInspectionRequestCancelHeader temp2 = new JobOrderInspectionRequestCancelHeaderService(_unitOfWork).Find(s.JobOrderInspectionRequestCancelHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status!=(int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestCancelHeader.Add(temp);
                    //new JobOrderInspectionRequestCancelHeaderService(_unitOfWork).Update(temp);

                    try
                    {
                        JobOrderInspectionRequestCancelDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestCancelHeaderId, EventModeConstants.Add), ref db);
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
                        JobOrderInspectionRequestCancelDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestCancelHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionRequestCancelHeaderId,
                        DocLineId = RequestLine.JobOrderInspectionRequestCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.JobOrderInspectionRequestCancelHeaderId, sid = svm.JobWorkerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                JobOrderInspectionRequestCancelHeader temp = new JobOrderInspectionRequestCancelHeaderService(db).Find(svm.JobOrderInspectionRequestCancelHeaderId);
                int status = temp.Status;

                JobOrderInspectionRequestCancelLine s = _JobOrderInspectionRequestCancelLineService.Find(svm.JobOrderInspectionRequestCancelLineId);


                JobOrderInspectionRequestCancelLine ExRec = new JobOrderInspectionRequestCancelLine();
                ExRec = Mapper.Map<JobOrderInspectionRequestCancelLine>(s);


                decimal balqty = (from p in db.ViewJobOrderInspectionRequestBalance
                                  where p.JobOrderInspectionRequestLineId == svm.JobOrderInspectionRequestLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }

                JobOrderInspectionRequestLine JobOrderInspectionRequestLine = new JobOrderInspectionRequestLineService(db).Find(s.JobOrderInspectionRequestLineId);
                JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = new JobOrderInspectionRequestHeaderService(db).Find(JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId);


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {

                        s.Remark = svm.Remark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                        //new JobOrderInspectionRequestLineStatusService(_unitOfWork).UpdateJobQtyOnRequest(s.JobOrderInspectionRequestLineId, s.JobOrderInspectionRequestCancelLineId, temp.DocDate, s.Qty, ref db, true);

                    }

                    //_JobOrderInspectionRequestCancelLineService.Update(s);
                    s.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestCancelLine.Add(s);

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
                    //new JobOrderInspectionRequestCancelHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestCancelHeader.Add(temp);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobOrderInspectionRequestCancelDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestCancelHeaderId, s.JobOrderInspectionRequestCancelLineId, EventModeConstants.Edit), ref db);
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
                        JobOrderInspectionRequestCancelDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionRequestCancelHeaderId, s.JobOrderInspectionRequestCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionRequestCancelHeaderId,
                        DocLineId = s.JobOrderInspectionRequestCancelLineId,
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
            JobOrderInspectionRequestCancelLineViewModel temp = _JobOrderInspectionRequestCancelLineService.GetJobOrderInspectionRequestCancelLine(id);
            JobOrderInspectionRequestCancelHeader header = new JobOrderInspectionRequestCancelHeaderService(db).Find(temp.JobOrderInspectionRequestCancelHeaderId);

            //Getting Settings
            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            var JobOrderInspectionRequestLine = new JobOrderInspectionRequestLineService(db).Find(temp.JobOrderInspectionRequestLineId);
            if (temp.ProductUidId.HasValue)
            {
                ViewBag.BarCodeGenerated = true;
                temp.BarCodes = temp.ProductUidId.ToString();
            }
            temp.JobOrderInspectionRequestSettings = Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
            if (string.IsNullOrEmpty(temp.LockReason))
                ViewBag.LineMode = "Edit";
            else
                TempData["CSEXCL"] += temp.LockReason;
            if (temp == null)
            {
                return HttpNotFound();
            }
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
            JobOrderInspectionRequestCancelLineViewModel temp = _JobOrderInspectionRequestCancelLineService.GetJobOrderInspectionRequestCancelLine(id);

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

            JobOrderInspectionRequestCancelHeader header = new JobOrderInspectionRequestCancelHeaderService(db).Find(temp.JobOrderInspectionRequestCancelHeaderId);

            //Getting Settings
            var settings = new JobOrderInspectionRequestSettingsService(db).GetJobOrderInspectionRequestSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            var JobOrderInspectionRequestLine = new JobOrderInspectionRequestLineService(db).Find(temp.JobOrderInspectionRequestLineId);

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
        public ActionResult DeletePost(JobOrderInspectionRequestCancelLineViewModel vm)
        {

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionRequestCancelDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobOrderInspectionRequestCancelHeaderId, vm.JobOrderInspectionRequestCancelLineId), ref db);
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

                JobOrderInspectionRequestCancelLine JobOrderInspectionRequestCancelLine = (from p in db.JobOrderInspectionRequestCancelLine
                                                                                           where p.JobOrderInspectionRequestCancelLineId == vm.JobOrderInspectionRequestCancelLineId
                                                                                           select p).FirstOrDefault();

                JobOrderInspectionRequestCancelHeader RequestHEader = new JobOrderInspectionRequestCancelHeaderService(db).Find(JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobOrderInspectionRequestCancelLine>(JobOrderInspectionRequestCancelLine),
                });

                //new JobOrderInspectionRequestLineStatusService(_unitOfWork).UpdateJobQtyOnRequest(JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestLineId, JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelLineId, RequestHEader.DocDate, 0, ref db, true);               


                //_JobOrderInspectionRequestCancelLineService.Delete(vm.JobOrderInspectionRequestCancelLineId);               

                JobOrderInspectionRequestCancelLine.ObjectState = Model.ObjectState.Deleted;
                db.JobOrderInspectionRequestCancelLine.Remove(JobOrderInspectionRequestCancelLine);

                JobOrderInspectionRequestCancelHeader header = new JobOrderInspectionRequestCancelHeaderService(db).Find(JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelHeaderId);
                if (header.Status != (int)StatusConstants.Drafted && header.Status!=(int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new JobOrderInspectionRequestCancelHeaderService(_unitOfWork).Update(header);
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionRequestCancelHeader.Add(header);
                }


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobOrderInspectionRequestCancelDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelHeaderId, JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelLineId), ref db);
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
                    JobOrderInspectionRequestCancelDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelHeaderId, JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = RequestHEader.DocTypeId,
                    DocId = RequestHEader.JobOrderInspectionRequestCancelHeaderId,
                    DocLineId = JobOrderInspectionRequestCancelLine.JobOrderInspectionRequestCancelLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                   
                    DocNo = RequestHEader.DocNo,
                    xEModifications = Modifications,
                    DocDate = RequestHEader.DocDate,
                    DocStatus=RequestHEader.Status,
                }));

            }
            return Json(new { success = true });
        }

        public JsonResult GetBarCodesForProductUid(int[] Id)
        {
            return Json(_JobOrderInspectionRequestCancelLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProductUidValidation(string ProductUID, int HeaderID)
        {
            return Json(_JobOrderInspectionRequestCancelLineService.ValidateInspectionRequestCancelBarCode(ProductUID, HeaderID), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRequestLineForUid(int UId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var JOLine = _JobOrderInspectionRequestCancelLineService.GetRequestLineForUidBranch(UId);
            if (JOLine == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(JOLine, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLineDetail(int LineId)
        {
            return Json( _JobOrderInspectionRequestCancelLineService.GetInspectionRequestLineDetail(LineId));
        }
        public JsonResult GetPendingRequest(int HeaderId, string term, int Limit)
        {
            return Json( _JobOrderInspectionRequestCancelLineService.GetPendingJobRequestsForAC(HeaderId, term, Limit).ToList());
        }
        public JsonResult CheckDuplicateJobOrder(int LineId, int RequestHeaderId)
        {
            return Json(_JobOrderInspectionRequestCancelLineService.CheckForDuplicateJobOrder(LineId, RequestHeaderId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPendingJobRequestProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionRequestCancelLineService.GetPendingProductsForJobOrderInspectionRequestCancel(searchTerm, filter);

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

            //return Json(_JobOrderInspectionRequestCancelLineService.GetPendingProductsForJobOrderInspectionRequestCancel(term, id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPendingJobRequests(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var Records = _JobOrderInspectionRequestCancelLineService.GetPendingJobRequests(searchTerm, filter);

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
            //return Json(_JobOrderInspectionRequestCancelLineService.GetPendingJobRequests("", term, id), JsonRequestBehavior.AllowGet);
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
