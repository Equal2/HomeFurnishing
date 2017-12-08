using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingReceiveController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobReceiveHeaderService _JobReceiveHeaderService;
        IJobReceiveLineService _JobReceiveLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public WeavingReceiveController(IJobReceiveLineService SaleOrder, IJobReceiveHeaderService JobReceiveHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveLineService = SaleOrder;
            _JobReceiveHeaderService = JobReceiveHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag(int id)
        {
            var Header = _JobReceiveHeaderService.Find(id);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(Header.DocTypeId).DocumentTypeName;
            ViewBag.DocNo = Header.DocNo;

            if (Header.Status == (int)StatusConstants.Drafted || Header.Status == (int)StatusConstants.Import)
            {
                ViewBag.Url = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Modify/" + id;
            }
            else if (Header.Status == (int)StatusConstants.Submitted || Header.Status == (int)StatusConstants.ModificationSubmitted || Header.Status == (int)StatusConstants.Modified)
            {
                ViewBag.Url = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/ModifyAfter_Submit/" + id;
            }
            else if (Header.Status == (int)StatusConstants.Approved || Header.Status == (int)StatusConstants.Closed)
            {
                ViewBag.Url = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/ModifyAfter_Approve/" + id;
            }

            ViewBag.ReasonList = _JobReceiveLineService.GetReasons(Header.DocTypeId);

        }


        [HttpGet]
        public ActionResult GetSummary(int id)
        {
            var Header = _JobReceiveHeaderService.Find(id);

            var JobReceives = (from p in db.JobReceiveLine
                               join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                               join RetLine in db.JobReturnLine on p.JobReceiveLineId equals RetLine.JobReceiveLineId
                               into ReturnLine
                               from RetLin in ReturnLine.DefaultIfEmpty()
                               join t2 in db.ViewRugArea on t.ProductId equals t2.ProductId into table
                               from tab in table.DefaultIfEmpty()
                               join t4 in db.Product on tab.ProductId equals t4.ProductId
                               join t6 in db.ProductGroups on t4.ProductGroupId equals t6.ProductGroupId
                               join t5 in db.Units on t4.UnitId equals t5.UnitId
                               where p.JobReceiveHeaderId == id && p.PassQty > 0
                               group new { t, p, tab, t5, t6, RetLin } by new { t.ProductId, t.JobOrderHeaderId } into g
                               orderby g.Max(m => m.t6.ProductGroupName), g.Max(m => m.t.UnitConversionMultiplier)
                               select new JobReceiveSummaryViewModel
                               {
                                   ProductName = g.Max(m => m.t.Product.ProductName),
                                   JobOrderHeaderId = g.Key.JobOrderHeaderId,
                                   JobOrderNo = g.Max(m => m.t.JobOrderHeader.DocNo),
                                   CostCenterName = g.Max(m => m.t.JobOrderHeader.CostCenter.CostCenterName),
                                   ProductId = g.Key.ProductId,
                                   Weight = (g.Sum(m => m.p.Weight) - g.Sum(m => m.RetLin == null ? 0 : (m.RetLin.Weight))),
                                   Qty = g.Sum(m => m.p.PassQty),
                                   ReturnQty = g.Sum(m => m.RetLin == null ? 0 : (m.RetLin.Qty)),
                                   UnitName = g.Max(m => m.t5.UnitName),
                                   MaxDecPlaces = g.Max(m => m.t5.DecimalPlaces),
                                   DealQty = g.Sum(m => m.p.PassQty * m.t.UnitConversionMultiplier),
                                   DealQtyPP = g.Max(m => m.t.UnitConversionMultiplier),
                                   MaxDealUnitDecPlaces = g.Max(m => m.t.DealUnit.DecimalPlaces),
                                   DealUnitName = g.Max(m => m.t.DealUnit.UnitName),
                                   ValidationError = (g.Where(m => m.p.Weight > 0 && m.RetLin == null).Any() && g.Where(m => m.p.Weight == 0 && m.RetLin == null).Any()),
                                   Penalty = g.Sum(m => m.p.PenaltyAmt) - (g.Sum(m => m.p.IncentiveAmt) ?? 0),
                               }).ToList();

            JobReceiveSummaryDetailViewModel vm = new JobReceiveSummaryDetailViewModel();
            vm.JobReceiveHeaderId = id;
            vm.DocData = Header.DocDate;
            vm.JobWorkerId = Header.JobWorkerId;
            vm.JobReceiveSummaryViewModel = JobReceives;
            PrepareViewBag(id);

            if (JobReceives.Count == 0)
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Index/" + Header.DocTypeId);
            else
                return View("Summary", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostSummary(JobReceiveSummaryDetailViewModel vm)
        {

            //TempData["CSEXC"] = "Customize Test Exception";

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool Modified = false;
            int Id = vm.JobReceiveHeaderId;

            var Header = _JobReceiveHeaderService.Find(Id);

            var JobReceives = (from p in db.JobReceiveLine
                               join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                               where p.JobReceiveHeaderId == Id
                               group t by new { t.ProductId, t.JobOrderHeaderId } into g
                               select g.Key).ToList();

            foreach (var item in vm.JobReceiveSummaryViewModel)
            {

                // Receive Line which has return also and return has no weight then receive weight should be 0.
                //var ReceiveWithReturnLines = from p in db.JobReceiveLine
                //                    join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                //                    join t2 in db.JobReceiveLineStatus on p.JobReceiveLineId equals t2.JobReceiveLineId into table
                //                    from tab in table.DefaultIfEmpty()
                //                    where t.JobOrderHeaderId == item.JobOrderHeaderId && p.PassQty > 0 && p.JobReceiveHeaderId == Id && t.ProductId == item.ProductId
                //                    && (p.PassQty - (tab.ReturnQty ?? 0)) == 0
                //                    select p;


                //foreach (var item3 in ReceiveWithReturnLines)
                //{

                //    item3.Weight = 0;
                //    item3.ObjectState = Model.ObjectState.Modified;
                //    _JobReceiveLineService.Update(item3);
                //}



                var ReceiveLines = (from p in db.JobReceiveLine
                                    join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                    join t2 in db.JobReceiveLineStatus on p.JobReceiveLineId equals t2.JobReceiveLineId into table
                                    from tab in table.DefaultIfEmpty()
                                    where t.JobOrderHeaderId == item.JobOrderHeaderId && p.PassQty > 0 && p.JobReceiveHeaderId == Id && t.ProductId == item.ProductId
                                    && (p.PassQty - (tab.ReturnQty ?? 0)) > 0
                                    select p).ToList();

                bool ValidationError = ReceiveLines.Where(m => m.Weight > 0).Any() && ReceiveLines.Where(m => m.Weight == 0).Any();

                if (ReceiveLines != null && ReceiveLines.Count > 0)
                {

                    decimal Weight = item.Weight;

                    decimal XWeight = ReceiveLines.Sum(m => m.Weight);

                    decimal PassQty = ReceiveLines.Sum(m => m.PassQty);


                    if (Weight != XWeight)
                    {

                        int i = 0;
                        decimal WeightShortage = 0;
                        decimal IndividualWeight = Math.Round((Weight / PassQty), 2);
                        if (i == 0 && IndividualWeight * PassQty != Weight)
                            WeightShortage = (Weight - (IndividualWeight * PassQty));

                        foreach (var item2 in ReceiveLines)
                        {
                            JobReceiveLine ExRec = new JobReceiveLine();
                            ExRec = Mapper.Map<JobReceiveLine>(item2);

                            item2.Weight = IndividualWeight * item2.PassQty + (i == 0 ? WeightShortage : 0);
                            item2.ModifiedBy = User.Identity.Name;
                            item2.ModifiedDate = DateTime.Now;
                            item2.ObjectState = Model.ObjectState.Modified;

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRec,
                                Obj = item2,
                            });

                            _JobReceiveLineService.Update(item2);
                            i++;

                            Modified = true;
                        }
                    }


                }



            }

            if ((Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import) && Modified)
            {
                Header.Status = (int)StatusConstants.Modified;
                Header.ModifiedBy = User.Identity.Name;
            }

            Header.ModifiedDate = DateTime.Now;
            Header.ObjectState = Model.ObjectState.Modified;
            new JobReceiveHeaderService(_unitOfWork).Update(Header);

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                PrepareViewBag(vm.JobReceiveHeaderId);
                return Json(new { Success = false });
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Header.DocTypeId,
                DocId = Header.JobReceiveHeaderId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = Header.DocNo,
                xEModifications = Modifications,
                DocDate = Header.DocDate,
                DocStatus = Header.Status,
            }));

            string RetUrl = "";


            if (Header.Status == (int)StatusConstants.Drafted || Header.Status == (int)StatusConstants.Import)
            {
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Modify/" + Header.JobReceiveHeaderId;
            }
            else if (Header.Status == (int)StatusConstants.Submitted || Header.Status == (int)StatusConstants.ModificationSubmitted || Header.Status == (int)StatusConstants.Modified)
            {
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/ModifyAfter_Submit/" + Header.JobReceiveHeaderId;
            }
            else if (Header.Status == (int)StatusConstants.Approved || Header.Status == (int)StatusConstants.Closed)
            {
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/ModifyAfter_Approve/" + Header.JobReceiveHeaderId;
            }
            else
                RetUrl = System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Index/" + Header.DocTypeId;

            return Json(new { Success = true, Url = RetUrl });
        }


        [HttpGet]
        public ActionResult GetBarCodesForIAP(int id)
        {
            var Header = _JobReceiveHeaderService.Find(id);

            var JobReceives = (from p in db.JobReceiveLine
                               join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                               join rl in db.JobReturnLine on p.JobReceiveLineId equals rl.JobReceiveLineId into retlinetable
                               from rlintab in retlinetable.DefaultIfEmpty()
                               join t2 in db.Product on t.ProductId equals t2.ProductId
                               join t3 in db.Units on t.DealUnitId equals t3.UnitId
                               join t4 in db.ProductUid on p.ProductUidId equals t4.ProductUIDId
                               where p.JobReceiveHeaderId == id
                               orderby p.Sr
                               select new JobReceiveIAPSummaryViewModel { ProductName = t2.ProductName, ProductUidId = t4.ProductUIDId, ProductUidName = t4.ProductUidName, DealQty = t.UnitConversionMultiplier * p.PassQty, DealUnitName = t3.UnitName, MaxDecPlaces = t3.DecimalPlaces, IncentiveAmt = p.IncentiveAmt ?? 0, PenalityAmt = p.PenaltyAmt, Remark = p.Remark, IsReturned=(rlintab!=null) }).ToList();

            JobReceiveIAPSummaryDetailViewModel vm = new JobReceiveIAPSummaryDetailViewModel();
            vm.JobReceiveHeaderId = id;
            vm.DocTypeId = Header.DocTypeId;
            vm.DocData = Header.DocDate;
            vm.JobWorkerId = Header.JobWorkerId;
            vm.JobReceiveIAPSummaryViewModel = JobReceives;
            PrepareViewBag(id);



            if (JobReceives.Count == 0)
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Index/" + Header.DocTypeId);
            else
                return View("SummaryIAP", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostIAPSummary(JobReceiveIAPSummaryDetailViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool Modified = false;

            int Id = vm.JobReceiveHeaderId;

            var Header = _JobReceiveHeaderService.Find(Id);

            int[] BarCodes = vm.JobReceiveIAPSummaryViewModel.Select(m => m.ProductUidId).ToArray();

            var ReceiveLines = (from p in db.JobReceiveLine
                                where p.JobReceiveHeaderId == Id && p.ProductUidId != null && BarCodes.Contains(p.ProductUidId.Value)
                                select p);

            foreach (var item in vm.JobReceiveIAPSummaryViewModel)
            {
                var ReceiveLine = ReceiveLines.Where(m => m.ProductUidId == item.ProductUidId).FirstOrDefault();

                if (ReceiveLine.PenaltyAmt != item.PenalityAmt || (ReceiveLine.IncentiveAmt != item.IncentiveAmt) || ReceiveLine.Remark != item.Remark)
                {
                    JobReceiveLine ExRec = new JobReceiveLine();
                    ExRec = Mapper.Map<JobReceiveLine>(ReceiveLine);                    
                    ReceiveLine.PenaltyAmt = item.PenalityAmt;
                    ReceiveLine.IncentiveAmt = item.IncentiveAmt;
                    ReceiveLine.Remark = item.Remark;
                    ReceiveLine.ModifiedBy = User.Identity.Name;
                    ReceiveLine.ModifiedDate = DateTime.Now;
                    ReceiveLine.ObjectState = Model.ObjectState.Modified;

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = ReceiveLine,
                    });

                    _JobReceiveLineService.Update(ReceiveLine);
                    Modified = true;

                }
            }


            if ((Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import) && Modified)
            {
                Header.Status = (int)StatusConstants.Modified;
                Header.ModifiedBy = User.Identity.Name;
            }

            Header.ObjectState = Model.ObjectState.Modified;
            new JobReceiveHeaderService(_unitOfWork).Update(Header);

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                PrepareViewBag(vm.JobReceiveHeaderId);
                return Json(new { Success = false });
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = Header.DocTypeId,
                DocId = Header.JobReceiveHeaderId,
                ActivityType = (int)ActivityTypeContants.Modified,
                DocNo = Header.DocNo,
                xEModifications = Modifications,
                DocDate = Header.DocDate,
                DocStatus = Header.Status,
            }));

            string RetUrl = "";

            if (Header.Status == (int)StatusConstants.Drafted || Header.Status == (int)StatusConstants.Import)
            {
                RetUrl = (System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Modify/" + Header.JobReceiveHeaderId);
            }
            else if (Header.Status == (int)StatusConstants.Submitted || Header.Status == (int)StatusConstants.ModificationSubmitted || Header.Status == (int)StatusConstants.Modified)
            {
                RetUrl = (System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/ModifyAfter_Submit/" + Header.JobReceiveHeaderId);
            }
            else if (Header.Status == (int)StatusConstants.Approved || Header.Status == (int)StatusConstants.Closed)
            {
                RetUrl = (System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/ModifyAfter_Approve/" + Header.JobReceiveHeaderId);
            }
            else
                RetUrl = (System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/JobReceiveHeader/Index/" + Header.DocTypeId);

            return Json(new { Success = true, Url = RetUrl });
        }
     
        public JsonResult SetReason(string Ids)
        {
            return Json(_JobReceiveLineService.SetReason(Ids));
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
