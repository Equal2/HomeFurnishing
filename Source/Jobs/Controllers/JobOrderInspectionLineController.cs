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
using DocumentEvents;
using CustomEventArgs;
using JobOrderInspectionDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class JobOrderInspectionLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;

        IJobOrderInspectionLineService _JobOrderInspectionLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobOrderInspectionLineController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderInspectionLineService = new JobOrderInspectionLineService(db);
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
            var p = _JobOrderInspectionLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }



        public ActionResult _ForOrder(int id, int sid)
        {
            JobOrderInspectionLineFilterViewModel vm = new JobOrderInspectionLineFilterViewModel();
            vm.JobOrderInspectionHeaderId = id;
            vm.JobWorkerId = sid;
            vm.JobWorkerId = sid;
            JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForRequest(int id, int sid)
        {
            JobOrderInspectionLineFilterViewModel vm = new JobOrderInspectionLineFilterViewModel();
            vm.JobOrderInspectionHeaderId = id;
            vm.JobWorkerId = sid;
            JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_RequestFilters", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderInspectionLineFilterViewModel vm)
        {
            List<JobOrderInspectionLineViewModel> temp = _JobOrderInspectionLineService.GetJobOrderLineForMultiSelect(vm).ToList();
            JobOrderInspectionMasterDetailModel svm = new JobOrderInspectionMasterDetailModel();
            svm.JobOrderInspectionLineViewModel = temp.Where(m => m.ProductUidId == null).ToList();
            JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.JobOrderInspectionHeaderId);
            JobOrderInspectionSettings settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.JobOrderInspectionSettings = Mapper.Map<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>(settings);

            if (temp.Where(m => m.ProductUidId != null).Any())
            {
                List<JobOrderInspectionLineViewModel> Sequence = new List<JobOrderInspectionLineViewModel>();
                Sequence = temp.Where(m => m.ProductUidId != null).ToList();
                foreach (var item in Sequence)
                    item.JobInspectionType = JobReceiveTypeConstants.ProductUid;

                HttpContext.Session["UIQUID" + vm.JobOrderInspectionHeaderId] = Sequence;
            }

            if (!temp.Where(m => m.ProductUidId == null).Any())
            {
                if (temp.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    List<BarCodeSequenceViewModelForInspection> Sequence = new List<BarCodeSequenceViewModelForInspection>();

                    var Grouping = (from p in temp.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobOrderHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspection
                        {
                            JobOrderInspectionHeaderId = item.Max(m => m.JobOrderInspectionHeaderId),
                            //JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId,
                            JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionType = JobReceiveTypeConstants.ProductUid,
                            JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                            // FirstBarCode = _JobOrderInspectionLineService.GetFirstBarCodeForCancel(item.JobOrderInspectionRequestLineId),
                        });

                    BarCodeSequenceListViewModelForInspection SquenceList = new BarCodeSequenceListViewModelForInspection();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

            }

            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _RequestFilterPost(JobOrderInspectionLineFilterViewModel vm)
        {
            List<JobOrderInspectionLineViewModel> temp = _JobOrderInspectionLineService.GetJobRequestLineForMultiSelect(vm).ToList();
            JobOrderInspectionMasterDetailModel svm = new JobOrderInspectionMasterDetailModel();
            svm.JobOrderInspectionLineViewModel = temp.Where(m => m.ProductUidId == null).ToList();
            JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.JobOrderInspectionHeaderId);
            JobOrderInspectionSettings settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            svm.JobOrderInspectionSettings = Mapper.Map<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>(settings);

            if (temp.Where(m => m.ProductUidId != null).Any())
            {
                List<JobOrderInspectionLineViewModel> Sequence = new List<JobOrderInspectionLineViewModel>();
                Sequence = temp.Where(m => m.ProductUidId != null).ToList();
                foreach (var item in Sequence)
                    item.JobInspectionType = JobReceiveTypeConstants.ProductUid;

                HttpContext.Session["UIQUID" + vm.JobOrderInspectionHeaderId] = Sequence;
            }

            if (!temp.Where(m => m.ProductUidId == null).Any())
            {
                if (temp.Where(m => m.ProductUidId != null).Count() > 0)
                {
                    List<BarCodeSequenceViewModelForInspection> Sequence = new List<BarCodeSequenceViewModelForInspection>();

                    var Grouping = (from p in temp.Where(m => m.ProductUidId != null)
                                    group p by new { p.JobOrderInspectionRequestHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspection
                        {
                            JobOrderInspectionHeaderId = item.Max(m => m.JobOrderInspectionHeaderId),
                            //JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId,
                            JobOrdInspectionRequestLineIds = string.Join(",", item.Select(m => m.JobOrderInspectionRequestLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionType = JobReceiveTypeConstants.ProductUid,
                            JobOrderInspectionRequestHeaderId = item.Max(m => m.JobOrderInspectionRequestHeaderId),
                            // FirstBarCode = _JobOrderInspectionLineService.GetFirstBarCodeForCancel(item.JobOrderInspectionRequestLineId),
                        });

                    BarCodeSequenceListViewModelForInspection SquenceList = new BarCodeSequenceListViewModelForInspection();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;
                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionHeaderId);

                    return PartialView("_RequestSequence2", SquenceList);
                }

            }

            return PartialView("_RequestResults", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderInspectionMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobOrderInspectionLineService.GetMaxSr(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobOrderInspectionLineViewModel> BarCodeBasedBranch = new List<JobOrderInspectionLineViewModel>();

            BarCodeBasedBranch = (List<JobOrderInspectionLineViewModel>)HttpContext.Session["UIQUID" + vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId];


            HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);

                JobOrderInspectionSettings Settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                var JobOrderLineIds = vm.JobOrderInspectionLineViewModel.Select(m => m.JobOrderLineId).ToArray();


                var JobOrderLineBalanceQtyRecords = (from p in db.ViewJobOrderBalanceForInspection
                                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                                     select new { BalQty = p.BalanceQty, LineId = p.JobOrderLineId }).ToList();

                var JobOrderLineRecords = (from p in db.JobOrderLine
                                           where JobOrderLineIds.Contains(p.JobOrderLineId)
                                           select p).ToList();

                var JobOrderHeaderIds = JobOrderLineRecords.Select(m => m.JobOrderHeaderId).ToArray();

                var JobOrderHeaderRecords = (from p in db.JobOrderHeader
                                             where JobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                             select p).ToList();

                var ProductUids = JobOrderLineRecords.Select(m => m.ProductUidId).ToArray();

                var ProductUidsRecords = (from p in db.ProductUid
                                          where ProductUids.Contains(p.ProductUIDId)
                                          select p).ToList();

                foreach (var item in vm.JobOrderInspectionLineViewModel.Where(m => m.Qty > 0))
                {
                    var balqty = JobOrderLineBalanceQtyRecords.Where(m => m.LineId == item.JobOrderLineId).FirstOrDefault().BalQty;

                    JobOrderLine JobOrderLine = JobOrderLineRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault();
                    JobOrderHeader JobOrderHeader = JobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty && JobOrderLine.ProductUidId == null)
                    {

                        JobOrderInspectionLine line = new JobOrderInspectionLine();
                        line.JobOrderInspectionLineId = -Cnt;
                        line.JobOrderInspectionHeaderId = item.JobOrderInspectionHeaderId;
                        line.JobOrderLineId = item.JobOrderLineId;
                        line.ProductUidId = JobOrderLine.ProductUidId;
                        line.Qty = item.Qty;
                        line.InspectedQty = item.InspectedQty;
                        line.Marks = item.Marks;
                        line.Remark = item.Remark;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        LineStatus.Add(line.JobOrderLineId, line.Qty);

                        line.ObjectState = Model.ObjectState.Added;
                        db.JobOrderInspectionLine.Add(line);

                        Cnt = Cnt + 1;

                    }
                }

                //new JobOrderInspectionHeaderService(_unitOfWork).Update(Header);
                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderInspectionHeader.Add(Header);
                //new JobOrderInspectionRequestLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                    JobOrderInspectionDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                                    group p by new { p.JobOrderHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspection
                        {
                            JobOrderInspectionHeaderId = item.Max(m => m.JobOrderInspectionHeaderId),
                            //JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId,
                            JobOrdLineIds = string.Join(",", item.Select(m => m.JobOrderLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionType = JobReceiveTypeConstants.ProductUid,
                            JobOrderHeaderId = item.Max(m => m.JobOrderHeaderId),
                            // FirstBarCode = _JobOrderInspectionLineService.GetFirstBarCodeForCancel(item.JobOrderInspectionRequestLineId),
                        });

                    BarCodeSequenceListViewModelForInspection SquenceList = new BarCodeSequenceListViewModelForInspection();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);

                    return PartialView("_Sequence2", SquenceList);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                   
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Results", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _RequestResultsPost(JobOrderInspectionMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _JobOrderInspectionLineService.GetMaxSr(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<JobOrderInspectionLineViewModel> BarCodeBasedBranch = new List<JobOrderInspectionLineViewModel>();

            BarCodeBasedBranch = (List<JobOrderInspectionLineViewModel>)HttpContext.Session["UIQUID" + vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId];


            HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);


            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);

                JobOrderInspectionSettings Settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

                var JobOrderInspectionRequestLineIds = vm.JobOrderInspectionLineViewModel.Select(m => m.JobOrderInspectionRequestLineId).ToArray();


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

                foreach (var item in vm.JobOrderInspectionLineViewModel.Where(m => m.Qty > 0))
                {
                    var balqty = JobOrderInspectionRequestLineBalanceQtyRecords.Where(m => m.LineId == item.JobOrderInspectionRequestLineId).FirstOrDefault().BalQty;

                    JobOrderInspectionRequestLine JobOrderInspectionRequestLine = JobOrderInspectionRequestLineRecords.Where(m => m.JobOrderInspectionRequestLineId == item.JobOrderInspectionRequestLineId).FirstOrDefault();
                    JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = JobOrderInspectionRequestHeaderRecords.Where(m => m.JobOrderInspectionRequestHeaderId == JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty && JobOrderInspectionRequestLine.ProductUidId == null)
                    {

                        JobOrderInspectionLine line = new JobOrderInspectionLine();
                        line.JobOrderInspectionLineId = -Cnt;
                        line.JobOrderInspectionHeaderId = item.JobOrderInspectionHeaderId;
                        line.JobOrderLineId = JobOrderInspectionRequestLine.JobOrderLineId;
                        line.JobOrderInspectionRequestLineId = JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId;
                        line.ProductUidId = JobOrderInspectionRequestLine.ProductUidId;
                        line.Qty = item.Qty;
                        line.InspectedQty = item.InspectedQty;
                        line.Marks = item.Marks;
                        line.Remark = item.Remark;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        LineStatus.Add(JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId, line.Qty);

                        line.ObjectState = Model.ObjectState.Added;
                        db.JobOrderInspectionLine.Add(line);

                        Cnt = Cnt + 1;

                    }
                }

                //new JobOrderInspectionHeaderService(_unitOfWork).Update(Header);
                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderInspectionHeader.Add(Header);
                //new JobOrderInspectionRequestLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                    return PartialView("_RequestResults", vm);
                }              


                try
                {
                    JobOrderInspectionDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                                    group p by new { p.JobOrderInspectionRequestHeaderId, p.ProductId } into g
                                    select g).ToList();

                    foreach (var item in Grouping)
                        Sequence.Add(new BarCodeSequenceViewModelForInspection
                        {
                            JobOrderInspectionHeaderId = item.Max(m => m.JobOrderInspectionHeaderId),
                            //JobOrderInspectionRequestLineId = item.JobOrderInspectionRequestLineId,
                            JobOrdInspectionRequestLineIds = string.Join(",", item.Select(m => m.JobOrderInspectionRequestLineId).ToList()),
                            ProductName = item.Max(m => m.ProductName),
                            Qty = item.Sum(m => m.BalanceQty),
                            BalanceQty = item.Sum(m => m.BalanceQty),
                            //CostCenterName = item.Max(m => m.CostCenterName),
                            JobOrderInspectionType = JobReceiveTypeConstants.ProductUid,
                            JobOrderInspectionRequestHeaderId = item.Max(m => m.JobOrderInspectionRequestHeaderId),
                            // FirstBarCode = _JobOrderInspectionLineService.GetFirstBarCodeForCancel(item.JobOrderInspectionRequestLineId),
                        });

                    BarCodeSequenceListViewModelForInspection SquenceList = new BarCodeSequenceListViewModelForInspection();
                    SquenceList.BarCodeSequenceViewModel = Sequence;

                    // System.Web.HttpContext.Current.Session[Header.DocNo + Header.DocTypeId + Header.DocDate] = Sequence;

                    HttpContext.Session.Remove("UIQUID" + vm.JobOrderInspectionLineViewModel.FirstOrDefault().JobOrderInspectionHeaderId);

                    return PartialView("_RequestSequence2", SquenceList);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                   
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_RequestResults", vm);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _SequencePost2(BarCodeSequenceListViewModelForInspection vm)
        {
            List<BarCodeSequenceViewModelForInspection> Seq = new List<BarCodeSequenceViewModelForInspection>();
            BarCodeSequenceListViewModelForInspection SquLis = new BarCodeSequenceListViewModelForInspection();

            JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderInspectionHeaderId);

            var LineIds = vm.BarCodeSequenceViewModel.Select(m => m.JobOrderLineId).ToArray();


            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
                {
                    string jOLIDs = item.JobOrdLineIds;

                    BarCodeSequenceViewModelForInspection SeqLi = new BarCodeSequenceViewModelForInspection();

                    SeqLi.JobOrderInspectionHeaderId = item.JobOrderInspectionHeaderId;
                    SeqLi.JobOrdLineIds = item.JobOrdLineIds;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobOrderInspectionType = item.JobOrderInspectionType;
                    SeqLi.JobOrderHeaderId = item.JobOrderHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobOrderInspectionLineService.GetPendingBarCodesList(SeqLi.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobOrderInspectionLineService.GetPendingBarCodesList(SeqLi.JobOrdLineIds.Split(',').Select(Int32.Parse).ToArray());

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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _RequestSequencePost2(BarCodeSequenceListViewModelForInspection vm)
        {
            List<BarCodeSequenceViewModelForInspection> Seq = new List<BarCodeSequenceViewModelForInspection>();
            BarCodeSequenceListViewModelForInspection SquLis = new BarCodeSequenceListViewModelForInspection();

            JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.BarCodeSequenceViewModel.FirstOrDefault().JobOrderInspectionHeaderId);

            foreach (var item in vm.BarCodeSequenceViewModel.Where(m => m.Qty > 0))
            {
                if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
                {
                    string jOLIDs = item.JobOrdInspectionRequestLineIds;

                    BarCodeSequenceViewModelForInspection SeqLi = new BarCodeSequenceViewModelForInspection();

                    SeqLi.JobOrderInspectionHeaderId = item.JobOrderInspectionHeaderId;
                    SeqLi.JobOrdInspectionRequestLineIds = item.JobOrdInspectionRequestLineIds;
                    SeqLi.ProductName = item.ProductName;
                    SeqLi.Qty = item.Qty;
                    SeqLi.JobOrderInspectionType = item.JobOrderInspectionType;
                    SeqLi.JobOrderInspectionRequestHeaderId = item.JobOrderInspectionRequestHeaderId;

                    if (!string.IsNullOrEmpty(item.ProductUidIdName))
                    {
                        var BarCodes = _JobOrderInspectionLineService.GetPendingBarCodesListForInspectionRequest(SeqLi.JobOrdInspectionRequestLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.SkipWhile(m => m.PropFirst != item.ProductUidIdName).Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }
                    else
                    {
                        var BarCodes = _JobOrderInspectionLineService.GetPendingBarCodesListForInspectionRequest(SeqLi.JobOrdInspectionRequestLineIds.Split(',').Select(Int32.Parse).ToArray());

                        var temp = BarCodes.Take((int)item.Qty);
                        string Ids = string.Join(",", temp.Select(m => m.Id));
                        SeqLi.ProductUidIds = Ids;
                    }

                    Seq.Add(SeqLi);

                }


            }
            SquLis.BarCodeSequenceViewModelPost = Seq;
            return PartialView("_RequestBarCodes", SquLis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _BarCodesPost(BarCodeSequenceListViewModelForInspection vm)
        {

            int Cnt = 0;
            int Pk = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            int Serial = _JobOrderInspectionLineService.GetMaxSr(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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

            var ProdUidJobOrderLineRecords = (from p in db.ViewJobOrderBalanceForInspection
                                              join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                              where ProdUidJobOrderLineIds.Contains(p.JobOrderLineId)
                                              select t).ToList();
            var ProdUidJobOrderHeaderIds = ProdUidJobOrderLineRecords.GroupBy(m => m.JobOrderHeaderId).Select(m => m.Key).ToArray();

            var ProdUidJobOrderHeaderRecords = (from p in db.JobOrderHeader
                                                where ProdUidJobOrderHeaderIds.Contains(p.JobOrderHeaderId)
                                                select p).ToList();

            var BalanceQtyRecords = (from p in db.ViewJobOrderBalanceForInspection
                                     where JobOrderLineIds.Contains(p.JobOrderLineId)
                                     select new { BalQty = p.BalanceQty, JobOrderLineId = p.JobOrderLineId }).ToList();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId);

                foreach (var item in vm.BarCodeSequenceViewModelPost)
                {

                    JobOrderLine JobOrderLine = new JobOrderLine();
                    JobOrderHeader JobOrderHeader = new JobOrderHeader();

                    decimal balqty = 0;

                    if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
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
                            if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
                            {
                                JobOrderLine = ProdUidJobOrderLineRecords.Where(m => m.ProductUidId == BarCodes && m.JobOrderHeaderId == item.JobOrderHeaderId).FirstOrDefault();
                                JobOrderHeader = ProdUidJobOrderHeaderRecords.Where(m => m.JobOrderHeaderId == JobOrderLine.JobOrderHeaderId).FirstOrDefault();
                            }

                            JobOrderInspectionLine line = new JobOrderInspectionLine();

                            line.JobOrderInspectionHeaderId = item.JobOrderInspectionHeaderId;

                            if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
                            {
                                line.JobOrderLineId = JobOrderLine.JobOrderLineId;
                            }
                            line.ProductUidId = BarCodes;
                            line.Qty = 1;
                            line.InspectedQty = 1;
                            line.Sr = Serial++;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobOrderInspectionLineId = Pk++;

                            if (LineStatus.ContainsKey(line.JobOrderLineId))
                            {
                                LineStatus[line.JobOrderLineId] = LineStatus[line.JobOrderLineId] + 1;
                            }
                            else
                            {
                                LineStatus.Add(line.JobOrderLineId, line.Qty);
                            }


                            line.ObjectState = Model.ObjectState.Added;
                            db.JobOrderInspectionLine.Add(line);

                            //new JobOrderInspectionLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderInspectionLineId, ref db, true);


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
                db.JobOrderInspectionHeader.Add(Header);
                //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                    JobOrderInspectionDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                  
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_BarCodes", vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _RequestBarCodesPost(BarCodeSequenceListViewModelForInspection vm)
        {

            int Cnt = 0;
            int Pk = 0;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            int Serial = _JobOrderInspectionLineService.GetMaxSr(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                JobOrderInspectionHeader Header = new JobOrderInspectionHeaderService(db).Find(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId);

                foreach (var item in vm.BarCodeSequenceViewModelPost)
                {

                    JobOrderInspectionRequestLine JobOrderInspectionRequestLine = new JobOrderInspectionRequestLine();
                    JobOrderInspectionRequestHeader JobOrderInspectionRequestHeader = new JobOrderInspectionRequestHeader();

                    decimal balqty = 0;

                    if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
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
                            if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
                            {
                                JobOrderInspectionRequestLine = ProdUidJobOrderInspectionRequestLineRecords.Where(m => m.ProductUidId == BarCodes && m.JobOrderInspectionRequestHeaderId == item.JobOrderInspectionRequestHeaderId).FirstOrDefault();
                                JobOrderInspectionRequestHeader = ProdUidJobOrderInspectionRequestHeaderRecords.Where(m => m.JobOrderInspectionRequestHeaderId == JobOrderInspectionRequestLine.JobOrderInspectionRequestHeaderId).FirstOrDefault();
                            }

                            JobOrderInspectionLine line = new JobOrderInspectionLine();

                            line.JobOrderInspectionHeaderId = item.JobOrderInspectionHeaderId;

                            if (item.JobOrderInspectionType == JobReceiveTypeConstants.ProductUid)
                            {
                                line.JobOrderInspectionRequestLineId = JobOrderInspectionRequestLine.JobOrderInspectionRequestLineId;
                                line.JobOrderLineId = JobOrderInspectionRequestLine.JobOrderLineId;
                            }
                            line.ProductUidId = BarCodes;
                            line.Qty = 1;
                            line.InspectedQty = 1;
                            line.Sr = Serial++;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            line.JobOrderInspectionLineId = Pk++;

                            if (LineStatus.ContainsKey(line.JobOrderLineId))
                            {
                                LineStatus[line.JobOrderLineId] = LineStatus[line.JobOrderLineId] + 1;
                            }
                            else
                            {
                                LineStatus.Add(line.JobOrderLineId, line.Qty);
                            }


                            line.ObjectState = Model.ObjectState.Added;
                            db.JobOrderInspectionLine.Add(line);

                            //new JobOrderInspectionLineStatusService(_unitOfWork).CreateLineStatus(line.JobOrderInspectionLineId, ref db, true);


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
                db.JobOrderInspectionHeader.Add(Header);
                //new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyReceiveMultiple(LineStatus, Header.DocDate, ref db, true);

                try
                {
                    JobOrderInspectionDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
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
                    return PartialView("_RequestBarCodes", vm);
                }

                try
                {
                    JobOrderInspectionDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.BarCodeSequenceViewModelPost.FirstOrDefault().JobOrderInspectionHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderInspectionHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                   
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_RequestBarCodes", vm);

        }
        private void PrepareViewBag(JobOrderInspectionLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            var temp = new JobOrderInspectionHeaderService(db).GetJobOrderInspectionHeader(vm.JobOrderInspectionHeaderId);
            ViewBag.DocNo = temp.DocTypeName + "-" + temp.DocNo;
        }

        [HttpGet]
        public ActionResult CreateLine(int Id, int JobWorkerId, bool InsReq)
        {
            return _Create(Id, JobWorkerId, InsReq);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int JobWorkerId, bool InsReq)
        {
            return _Create(Id, JobWorkerId, InsReq);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int JobWorkerId, bool InsReq)
        {
            return _Create(Id, JobWorkerId, InsReq);
        }

        public ActionResult _Create(int Id, int JobWorkerId, bool InsReq) //Id ==> Header Id
        {
            JobOrderInspectionHeader H = new JobOrderInspectionHeaderService(db).Find(Id);
            JobOrderInspectionLineViewModel s = new JobOrderInspectionLineViewModel();

            //Getting Settings
            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.JobOrderInspectionSettings = Mapper.Map<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.JobOrderInspectionHeaderId = Id;
            s.JobOrderInspectionDocNo = H.DocNo;
            s.JobWorkerId = JobWorkerId;
            s.InsReq = InsReq;
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
        public ActionResult _CreatePost(JobOrderInspectionLineViewModel svm)
        {
            JobOrderInspectionLine s = Mapper.Map<JobOrderInspectionLineViewModel, JobOrderInspectionLine>(svm);
            JobOrderInspectionHeader temp = new JobOrderInspectionHeaderService(db).Find(s.JobOrderInspectionHeaderId);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (svm.JobOrderInspectionLineId <= 0)
                    BeforeSave = JobOrderInspectionDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderInspectionLineId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobOrderInspectionDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderInspectionLineId, EventModeConstants.Edit), ref db);

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


            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (svm.JobOrderInspectionLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty field is required");

            if (svm.InspectedQty <= 0)
                ModelState.AddModelError("InspectedQty", "The InspectedQty field is required");

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation Error before save.");

            if (svm.JobOrderLineId <= 0)
                ModelState.AddModelError("JobOrderLineId", "The JobOrderLine field is required");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.JobOrderInspectionLineId <= 0)
                {

                    JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(s.JobOrderLineId);
                    JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    s.Sr = _JobOrderInspectionLineService.GetMaxSr(s.JobOrderInspectionHeaderId);
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive(s.JobOrderLineId, s.JobOrderInspectionLineId, temp.DocDate, s.Qty, ref db);


                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                    }

                    //new JobOrderInspectionHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionHeader.Add(temp);

                    s.ObjectState = Model.ObjectState.Added;
                    db.JobOrderInspectionLine.Add(s);

                    //new JobOrderInspectionLineStatusService(_unitOfWork).CreateLineStatus(s.JobOrderInspectionLineId, ref db, true);

                    try
                    {
                        JobOrderInspectionDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobOrderInspectionHeaderId, s.JobOrderInspectionLineId, EventModeConstants.Add), ref db);
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
                        JobOrderInspectionDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobOrderInspectionHeaderId, s.JobOrderInspectionLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionHeaderId,
                        DocLineId = s.JobOrderInspectionLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                       
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.JobOrderInspectionHeaderId, JobWorkerId = svm.JobWorkerId, InsReq = svm.InsReq });
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    StringBuilder logstring = new StringBuilder();
                    JobOrderInspectionLine temp1 = _JobOrderInspectionLineService.Find(svm.JobOrderInspectionLineId);


                    JobOrderInspectionLine ExRec = new JobOrderInspectionLine();
                    ExRec = Mapper.Map<JobOrderInspectionLine>(temp1);


                    JobOrderLine JobOrderLine = new JobOrderLineService(_unitOfWork).Find(s.JobOrderLineId);
                    JobOrderHeader JobOrderHeader = new JobOrderHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderHeaderId);

                    temp1.Remark = svm.Remark;
                    temp1.Qty = svm.Qty;
                    temp1.InspectedQty = svm.InspectedQty;
                    temp1.Marks = svm.Marks;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;


                    new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive(s.JobOrderLineId, s.JobOrderInspectionLineId, temp.DocDate, s.Qty, ref db);

                    //_JobOrderInspectionLineService.Update(temp1);

                    db.JobOrderInspectionLine.Add(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    //new JobOrderInspectionHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionHeader.Add(temp);


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobOrderInspectionDocEvents.onLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionHeaderId, temp1.JobOrderInspectionLineId, EventModeConstants.Edit), ref db);
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
                        JobOrderInspectionDocEvents.afterLineSaveEvent(this, new JobEventArgs(temp.JobOrderInspectionHeaderId, temp1.JobOrderInspectionLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionHeaderId,
                        DocLineId = temp1.JobOrderInspectionLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus=temp.Status,
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
            JobOrderInspectionLineViewModel temp = _JobOrderInspectionLineService.GetJobOrderInspectionLine(id);

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

            JobOrderInspectionHeader H = new JobOrderInspectionHeaderService(db).Find(temp.JobOrderInspectionHeaderId);
            if (temp.JobOrderInspectionRequestLineId.HasValue && temp.JobOrderInspectionRequestLineId.Value != 0)
                temp.InsReq = true;
            //Getting Settings
            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobOrderInspectionSettings = Mapper.Map<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>(settings);

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
            JobOrderInspectionLineViewModel temp = _JobOrderInspectionLineService.GetJobOrderInspectionLine(id);

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

            JobOrderInspectionHeader H = new JobOrderInspectionHeaderService(db).Find(temp.JobOrderInspectionHeaderId);

            if (temp.JobOrderInspectionRequestLineId.HasValue && temp.JobOrderInspectionRequestLineId.Value != 0)
                temp.InsReq = true;

            //Getting Settings
            var settings = new JobOrderInspectionSettingsService(db).GetJobOrderInspectionSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.JobOrderInspectionSettings = Mapper.Map<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            PrepareViewBag(temp);

            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobOrderInspectionLineViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderInspectionDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobOrderInspectionHeaderId, vm.JobOrderInspectionLineId), ref db);
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


                JobOrderInspectionLine JobOrderInspectionLine = (from p in db.JobOrderInspectionLine
                                                                 where p.JobOrderInspectionLineId == vm.JobOrderInspectionLineId
                                                                 select p).FirstOrDefault();

                JobOrderInspectionHeader header = new JobOrderInspectionHeaderService(db).Find(JobOrderInspectionLine.JobOrderInspectionHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobOrderInspectionLine>(JobOrderInspectionLine),
                });

                new JobOrderLineStatusService(_unitOfWork).UpdateJobQtyOnReceive(JobOrderInspectionLine.JobOrderLineId, JobOrderInspectionLine.JobOrderInspectionLineId, header.DocDate, 0, ref db);


                //_JobOrderInspectionLineService.Delete(JobOrderInspectionLine);
                JobOrderInspectionLine.ObjectState = Model.ObjectState.Deleted;
                db.JobOrderInspectionLine.Remove(JobOrderInspectionLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderInspectionHeader.Add(header);
                    //new JobOrderInspectionHeaderService(_unitOfWork).Update(header);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    JobOrderInspectionDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobOrderInspectionLine.JobOrderInspectionHeaderId, JobOrderInspectionLine.JobOrderInspectionLineId), ref db);
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
                    JobOrderInspectionDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobOrderInspectionLine.JobOrderInspectionHeaderId, JobOrderInspectionLine.JobOrderInspectionLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.JobOrderInspectionHeaderId,
                    DocLineId = JobOrderInspectionLine.JobOrderInspectionLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,                   
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus=header.Status,
                }));

            }

            return Json(new { success = true });
        }

        public JsonResult GetBarCodesForProductUid(int[] Id)
        {
            return Json(_JobOrderInspectionLineService.GetPendingBarCodesList(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOrderDetail(int OrderId, int ReceiveId, bool InsReq)
        {
            return Json(_JobOrderInspectionLineService.GetLineDetailForInspection(OrderId, ReceiveId, InsReq));
        }
        public JsonResult GetPendingOrders(int HeaderId, string term, int Limit, bool InsReq)
        {
            return Json(_JobOrderInspectionLineService.GetPendingJobOrdersForAC(HeaderId, term, Limit, InsReq).ToList());
        }

        public JsonResult GetProductUidValidation(string ProductUID, int HeaderID)
        {
            return Json(_JobOrderInspectionLineService.ValidateInspectionBarCode(ProductUID, HeaderID), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetOrderLineForUid(int UId)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var JOLine = _JobOrderInspectionLineService.GetOrderLineForUid(UId);
            if (JOLine == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(JOLine, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPendingJobOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionLineService.GetPendingProductsForJobOrderInspection(searchTerm, filter);

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

            //return Json(_JobOrderInspectionLineService.GetPendingProductsForJobOrderInspection("", term, id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionLineService.GetPendingJobOrders(searchTerm, filter);

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
           // return Json(_JobOrderInspectionLineService.GetPendingJobOrders("", term, id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBarCodesForInspectionRequest(int[] Id)
        {
            return Json(_JobOrderInspectionLineService.GetPendingBarCodesListForInspectionRequest(Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingInspectionRequestProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionLineService.GetPendingJobOrders(searchTerm, filter);

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
            //return Json(_JobOrderInspectionLineService.GetPendingProductsForInspection("", term, id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPendingJobRequests(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Records = _JobOrderInspectionLineService.GetPendingJobRequests(searchTerm, filter);

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
            //return Json(_JobOrderInspectionLineService.GetPendingJobRequests("", term, id), JsonRequestBehavior.AllowGet);
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
