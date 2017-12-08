using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using AutoMapper;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using JobReceiveQADocumentEvents;
using CustomEventArgs;
using DocumentEvents;



namespace Jobs.Controllers
{
    [Authorize]
    public class JobReceiveQAWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobReceiveQAHeaderService _JobReceiveQAHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobReceiveQAWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveQAHeaderService = new JobReceiveQAHeaderService(db);
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!JobReceiveQAEvents.Initialized)
            {
                JobReceiveQAEvents Obj = new JobReceiveQAEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag(int id)
        {
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.id = id;
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList(DocType.DocumentTypeName).ToList();

        }
        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("ReceiveQAWizard", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult ReceiveQAWizard(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobReceiveQAHeaderViewModel vm = new JobReceiveQAHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobReceiveQA", "JobReceiveQASettings", new { id = id }).Warning("Please create Purchase order cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            ViewBag.ProcId = settings.ProcessId;

            return View();
        }

        public JsonResult AjaxGetJsonData(int DocType, DateTime? FromDate, DateTime? ToDate, string JobReceiveHeaderId, string JobWorkerId
            , string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? BalanceQty, decimal InspectionQty
            , decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            string SortColName = "";


            // note: we only sort one column at a time
            if (Request.Form["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
                SortColName = Request.Form["columns[" + sortColumn + "][data]"];
            }
            if (Request.Form["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            bool Success = true;

            var data = FilterData(DocType, FromDate, ToDate, JobReceiveHeaderId, JobWorkerId,
                                            ProductId, Dimension1Id, Dimension2Id, ProductGroupId, ProductCategoryId, BalanceQty, InspectionQty, MultiplierGT, MultiplierLT, Sample);

            var RecCount = data.Count();

            if (RecCount > 1000 || RecCount == 0)
            {
                Success = false;
                return Json(new { Success = Success, Message = (RecCount > 1000 ? "No of records exceeding 1000." : "No Records found.") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CList = data.ToList().Select(m => new JobReceiveQAWizardViewModel
                {
                    SOrderDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                    JobReceiveLineId = m.JobReceiveLineId,
                    OrderNo = m.OrderNo,
                    JobWorkerName = m.JobWorkerName,
                    ProductName = m.ProductName,
                    JobWorkerId = m.JobWorkerId,
                    Dimension1Name = m.Dimension1Name,
                    Dimension2Name = m.Dimension2Name,
                    BalanceQty = m.BalanceQty,
                    Qty = m.Qty,
                    PenaltyAmount = m.PenaltyAmount,
                    ProductUidName = m.ProductUidName,
                    InspectionQty = m.InspectionQty,
                    ProductGroupName = m.ProductGroupName
                }).ToList();

                return Json(new { Data = CList, Success = Success }, JsonRequestBehavior.AllowGet);
            }
        }


        private static int TOTAL_ROWS = 0;
        //private static readonly List<DataItem> _data = CreateData();    
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<JobReceiveQAWizardViewModel> data { get; set; }
        }

        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = int.Parse(s1);
            int i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }

        // here we simulate SQL search, sorting and paging operations
        private IQueryable<JobReceiveQAWizardViewModel> FilterData(int DocType, DateTime? FromDate, DateTime? ToDate,
                                                                    string JobReceiveHeaderId, string JobWorkerId, string ProductId, string Dimension1Id,
            string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? BalanceQty, decimal InspectionQty, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {

            List<int> JobReceiveHeaderIds = new List<int>();
            if (!string.IsNullOrEmpty(JobReceiveHeaderId))
                foreach (var item in JobReceiveHeaderId.Split(','))
                    JobReceiveHeaderIds.Add(Convert.ToInt32(item));


            List<int> JobWorkerIds = new List<int>();
            if (!string.IsNullOrEmpty(JobWorkerId))
                foreach (var item in JobWorkerId.Split(','))
                    JobWorkerIds.Add(Convert.ToInt32(item));

            //List<int> ProductIds = new List<int>();
            //if (!string.IsNullOrEmpty(ProductId))
            //    foreach (var item in ProductId.Split(','))
            //        ProductIds.Add(Convert.ToInt32(item));

            List<int> Dimension1Ids = new List<int>();
            if (!string.IsNullOrEmpty(Dimension1Id))
                foreach (var item in Dimension1Id.Split(','))
                    Dimension1Ids.Add(Convert.ToInt32(item));

            List<int> Dimension2Ids = new List<int>();
            if (!string.IsNullOrEmpty(Dimension2Id))
                foreach (var item in Dimension2Id.Split(','))
                    Dimension2Ids.Add(Convert.ToInt32(item));

            //List<int> ProductGroupIds = new List<int>();
            //if (!string.IsNullOrEmpty(ProductGroupId))
            //    foreach (var item in ProductGroupId.Split(','))
            //        ProductGroupIds.Add(Convert.ToInt32(item));

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var Settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(DocType, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            IQueryable<JobReceiveQAWizardViewModel> _data = from p in db.ViewJobReceiveBalanceForQA
                                                            join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                                                            join jrh in db.JobReceiveHeader on p.JobReceiveHeaderId equals jrh.JobReceiveHeaderId
                                                            join jw in db.Persons on p.JobWorkerId equals jw.PersonID into jwtable
                                                            from jwtab in jwtable.DefaultIfEmpty()
                                                            join prod in db.FinishedProduct on p.ProductId equals prod.ProductId into prodtable
                                                            from prodtab in prodtable.DefaultIfEmpty()
                                                            join jol in db.JobOrderLine on t.JobOrderLineId equals jol.JobOrderLineId
                                                            join dim1 in db.Dimension1 on jol.Dimension1Id equals dim1.Dimension1Id into dimtable
                                                            from dimtab in dimtable.DefaultIfEmpty()
                                                            join dim2 in db.Dimension2 on jol.Dimension2Id equals dim2.Dimension2Id into dim2table
                                                            from dim2tab in dim2table.DefaultIfEmpty()
                                                            join pg in db.ProductGroups on prodtab.ProductGroupId equals pg.ProductGroupId into pgtable
                                                            from pgtab in pgtable.DefaultIfEmpty()
                                                            join pc in db.ProductCategory on prodtab.ProductCategoryId equals pc.ProductCategoryId into pctable
                                                            from pctab in pctable.DefaultIfEmpty()
                                                            join ProdUid in db.ProductUid on p.ProductUidId equals ProdUid.ProductUIDId
                                                            into produidtable
                                                            from produidtab in produidtable.DefaultIfEmpty()
                                                            where p.BalanceQty > 0 && jrh.ProcessId == Settings.ProcessId
                                                            select new JobReceiveQAWizardViewModel
                                                         {
                                                             OrderDate = p.OrderDate,
                                                             OrderNo = p.JobReceiveNo,
                                                             JobReceiveLineId = p.JobReceiveLineId,
                                                             BalanceQty = p.BalanceQty,
                                                             Qty = p.BalanceQty,
                                                             InspectionQty = p.BalanceQty,
                                                             JobWorkerName = jwtab.Name,
                                                             ProductName = prodtab.ProductName,
                                                             Dimension1Name = dimtab.Dimension1Name,
                                                             Dimension2Name = dim2tab.Dimension2Name,
                                                             JobReceiveHeaderId = p.JobReceiveHeaderId,
                                                             JobWorkerId = p.JobWorkerId,
                                                             ProductGroupId = pgtab.ProductGroupId,
                                                             ProductGroupName = pgtab.ProductGroupName,
                                                             ProductCategoryId = pctab.ProductCategoryId,
                                                             ProductCategoryName = pctab.ProductCategoryName,
                                                             ProdId = p.ProductId,
                                                             Dimension1Id = dimtab.Dimension1Id,
                                                             Dimension2Id = dim2tab.Dimension2Id,
                                                             UnitConversionMultiplier = jol.UnitConversionMultiplier,
                                                             DocTypeId = p.DocTypeId,
                                                             ProductUidName = produidtab.ProductUidName,
                                                             PenaltyAmount = t.PenaltyAmt,
                                                         };



            //if (FromDate.HasValue)
            //    _data = from p in _data
            //            where p.OrderDate >= FromDate
            //            select p;


            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes))
                _data = _data.Where(m => contraDocTypes.Contains(m.DocTypeId.ToString()));

            if (FromDate.HasValue)
                _data = _data.Where(m => m.OrderDate >= FromDate);

            if (ToDate.HasValue)
                _data = _data.Where(m => m.OrderDate <= ToDate);

            if (BalanceQty.HasValue && BalanceQty.Value > 0)
                _data = _data.Where(m => m.BalanceQty == BalanceQty.Value);

            if (MultiplierGT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier >= MultiplierGT.Value);

            if (MultiplierLT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier <= MultiplierLT.Value);


            if (!string.IsNullOrEmpty(JobReceiveHeaderId))
                _data = _data.Where(m => JobReceiveHeaderIds.Contains(m.JobReceiveHeaderId));

            if (!string.IsNullOrEmpty(JobWorkerId))
                _data = _data.Where(m => JobWorkerIds.Contains(m.JobWorkerId));

            if (!string.IsNullOrEmpty(ProductId))
                _data = _data.Where(m => m.ProductName.Contains(ProductId));

            if (!string.IsNullOrEmpty(Dimension1Id))
                _data = _data.Where(m => Dimension1Ids.Contains(m.Dimension1Id ?? 0));

            if (!string.IsNullOrEmpty(Dimension2Id))
                _data = _data.Where(m => Dimension2Ids.Contains(m.Dimension2Id ?? 0));

            if (!string.IsNullOrEmpty(ProductGroupId))
                _data = _data.Where(m => m.ProductGroupName.Contains(ProductGroupId));

            if (!string.IsNullOrEmpty(ProductCategoryId))
                _data = _data.Where(m => m.ProductCategoryName.Contains(ProductCategoryId));

            //if (!string.IsNullOrEmpty(Sample) && Sample != "Include")
            //{
            //    if (Sample == "Exclude")
            //        _data = _data.Where(m => m.Sample == false);
            //    else if (Sample == "Only")
            //        _data = _data.Where(m => m.Sample == true);
            //}

            _data = _data.OrderBy(m => m.OrderDate).ThenBy(m => m.OrderNo);

            // get just one page of data
            return _data.Select(m => new JobReceiveQAWizardViewModel
            {
                OrderDate = m.OrderDate,
                OrderNo = m.OrderNo,
                JobReceiveLineId = m.JobReceiveLineId,
                BalanceQty = m.BalanceQty,
                Qty = m.Qty,
                InspectionQty = m.InspectionQty,
                JobWorkerName = m.JobWorkerName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                JobReceiveHeaderId = m.JobReceiveHeaderId,
                JobWorkerId = m.JobWorkerId,
                ProductGroupId = m.ProductGroupId,
                ProductGroupName = m.ProductGroupName,
                ProductCategoryId = m.ProductCategoryId,
                ProductCategoryName = m.ProductCategoryName,
                ProdId = m.ProdId,
                Dimension1Id = m.Dimension1Id,
                Dimension2Id = m.Dimension2Id,
                UnitConversionMultiplier = m.UnitConversionMultiplier,
                DocTypeId = m.DocTypeId,
                ProductUidName = m.ProductUidName,
                PenaltyAmount = m.PenaltyAmount,
            });
        }


        public ActionResult ConfirmedJobReceives(List<JobReceiveQAWizardViewModel> ConfirmedList, int DocTypeId, string UserRemark, int QAById)
        {
            //System.Web.HttpContext.Current.Session["BalanceQtyAmendmentWizardOrders"] = ConfirmedList;
            //return Json(new { Success = "URL", Data = "/JobReceiveQAWizard/Create/" + DocTypeId }, JsonRequestBehavior.AllowGet);

            if (ConfirmedList.Count() > 0 && ConfirmedList.GroupBy(m => m.JobWorkerId).Count() > 1)
                return Json(new { Success = false, Data = " Multiple Headers are selected. " }, JsonRequestBehavior.AllowGet);
            else if (ConfirmedList.Count() == 0)
                return Json(new { Success = false, Data = " No Records are selected. " }, JsonRequestBehavior.AllowGet);
            else
            {

                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

                bool BeforeSave = true;
                int Serial = 1;
                List<JobReceiveQALineViewModel> LineStatus = new List<JobReceiveQALineViewModel>();

                try
                {
                    BeforeSave = JobReceiveQADocEvents.beforeWizardSaveEvent(this, new JobEventArgs(0), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { Success = false, Data = message }, JsonRequestBehavior.AllowGet);
                }


                if (!BeforeSave)
                    TempData["CSEXC"] += "Failed validation before save";


                int Cnt = 0;
                int Sr = 0;


                JobReceiveQASettings Settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(DocTypeId, DivisionId, SiteId);

                int? MaxLineId = 0;

                if (ModelState.IsValid && BeforeSave && !EventException)
                {

                    JobReceiveQAHeader pt = new JobReceiveQAHeader();

                    //Getting Settings
                    pt.SiteId = SiteId;
                    pt.JobWorkerId = ConfirmedList.FirstOrDefault().JobWorkerId;
                    pt.DivisionId = DivisionId;
                    pt.QAById = QAById;
                    pt.ProcessId = Settings.ProcessId;
                    pt.Remark = UserRemark;
                    pt.DocTypeId = DocTypeId;
                    pt.DocDate = DateTime.Now;
                    pt.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveQAHeaders", pt.DocTypeId, pt.DocDate, pt.DivisionId, pt.SiteId);
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.CreatedDate = DateTime.Now;

                    pt.Status = (int)StatusConstants.Drafted;
                    pt.ObjectState = Model.ObjectState.Added;

                    db.JobReceiveQAHeader.Add(pt);

                    var SelectedJobReceives = ConfirmedList;

                    var JobReceiveLineIds = SelectedJobReceives.Select(m => m.JobReceiveLineId).ToArray();

                    var JobReceiveBalanceRecords = (from p in db.ViewJobReceiveBalanceForQA
                                                    where JobReceiveLineIds.Contains(p.JobReceiveLineId)
                                                    select p).AsNoTracking().ToList();

                    var JobReceiveRecords = (from p in db.JobReceiveLine.Include(m=>m.JobOrderLine)
                                             where JobReceiveLineIds.Contains(p.JobReceiveLineId)
                                             select p).AsNoTracking().ToList();

                    foreach (var item in SelectedJobReceives)
                    {
                        JobReceiveLine Recline = JobReceiveRecords.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault();
                        var balRecline = JobReceiveBalanceRecords.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault();

                        if (item.InspectionQty <= JobReceiveBalanceRecords.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault().BalanceQty)
                        {
                            JobReceiveQALine line = new JobReceiveQALine();

                            line.JobReceiveQAHeaderId = pt.JobReceiveQAHeaderId;
                            line.JobReceiveLineId = item.JobReceiveLineId;
                            line.QAQty = balRecline.BalanceQty;
                            line.UnitConversionMultiplier = Recline.JobOrderLine.UnitConversionMultiplier;
                            line.Qty = item.Qty;
                            line.DealQty = line.Qty * line.UnitConversionMultiplier;
                            line.FailQty = line.QAQty - line.Qty;
                            line.FailDealQty = line.FailQty * line.UnitConversionMultiplier;
                            line.InspectedQty = item.InspectionQty;
                            line.PenaltyAmt = item.PenaltyAmount;
                            line.Remark = item.Remark;
                            line.ProductUidId = Recline.ProductUidId;
                            line.Sr = Serial++;
                            line.JobReceiveQALineId = Cnt;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            LineStatus.Add(Mapper.Map<JobReceiveQALineViewModel>(line));

                            line.ObjectState = Model.ObjectState.Added;
                            db.JobReceiveQALine.Add(line);
                            Cnt = Cnt + 1;

                        }
                    }

                    new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyQAMultiple(LineStatus, pt.DocDate, ref db);

                    try
                    {
                        JobReceiveQADocEvents.onWizardSaveEvent(this, new JobEventArgs(pt.JobReceiveQAHeaderId, EventModeConstants.Add), ref db);
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
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        return Json(new { Success = false, Data = message }, JsonRequestBehavior.AllowGet);
                    }

                    try
                    {
                        JobReceiveQADocEvents.afterWizardSaveEvent(this, new JobEventArgs(pt.JobReceiveQAHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobReceiveQAHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        User = User.Identity.Name,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate
                    });

                    return Json(new { Success = "URL", Data = "/JobReceiveQAHeader/Submit/" + pt.JobReceiveQAHeaderId }, JsonRequestBehavior.AllowGet);


                }

                else
                    return Json(new { Success = false, Data = "ModelState is Invalid" }, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult SelectedRecords(List<JobReceiveQAWizardViewModel> SelectedRecords)
        {
            var OrderIds = SelectedRecords.Select(m => m.JobReceiveLineId).ToArray();
            var RecordDetails = (from p in db.ViewJobReceiveBalanceForQA
                                 join t in db.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                                 join h in db.JobReceiveHeader on t.JobReceiveHeaderId equals h.JobReceiveHeaderId
                                 join jol in db.JobOrderLine on t.JobOrderLineId equals jol.JobOrderLineId
                                 where OrderIds.Contains(p.JobReceiveLineId)
                                 select new JobReceiveQAWizardViewModel
                                 {
                                     OrderDate = p.OrderDate,
                                     OrderNo = p.JobReceiveNo,
                                     JobReceiveLineId = p.JobReceiveLineId,
                                     BalanceQty = p.BalanceQty,
                                     JobWorkerName = h.JobWorker.Name,
                                     ProductName = jol.Product.ProductName,
                                     Dimension1Name = jol.Dimension1.Dimension1Name,
                                     Dimension2Name = jol.Dimension2.Dimension2Name,
                                     ProductGroupName = jol.Product.ProductGroup.ProductGroupName

                                 }).ToList();

            var RecordDetailList = RecordDetails.Select(m => new JobReceiveQAWizardViewModel
            {
                SOrderDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                JobReceiveLineId = m.JobReceiveLineId,
                OrderNo = m.OrderNo,
                JobWorkerName = m.JobWorkerName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                BalanceQty = m.BalanceQty,
                InspectionQty = SelectedRecords.Where(t => t.JobReceiveLineId == m.JobReceiveLineId).FirstOrDefault().InspectionQty,
                ProductGroupName = m.ProductGroupName
            }).ToList();

            return PartialView("_SelectedRecords", RecordDetailList);

        }




        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobReceiveQAHeaderViewModel vm = new JobReceiveQAHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, vm.DivisionId, vm.SiteId);
            vm.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveQAHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }



        public ActionResult Filters(int DocTypeId, DateTime? FromDate, DateTime? ToDate,
            string JobReceiveHeaderId, string JobWorkerId, string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId,
            string ProductCategoryId, decimal? BalanceQty, decimal InspectionQty, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {
            JobReceiveQAWizardFilterViewModel vm = new JobReceiveQAWizardFilterViewModel();

            List<SelectListItem> tempSOD = new List<SelectListItem>();
            tempSOD.Add(new SelectListItem { Text = "Include Sample", Value = "Include" });
            tempSOD.Add(new SelectListItem { Text = "Exculde Sample", Value = "Exculde" });
            tempSOD.Add(new SelectListItem { Text = "Only Sample", Value = "Only" });

            ViewBag.SOD = new SelectList(tempSOD, "Value", "Text", Sample);


            vm.DocTypeId = DocTypeId;
            vm.FromDate = FromDate;
            vm.ToDate = ToDate;
            vm.JobReceiveHeaderId = JobReceiveHeaderId;
            vm.JobWorkerId = JobWorkerId;
            vm.ProductId = ProductId;
            vm.Dimension1Id = Dimension1Id;
            vm.Dimension2Id = Dimension2Id;
            vm.ProductGroupId = ProductGroupId;
            vm.ProductCategoryId = ProductCategoryId;
            vm.BalanceQty = BalanceQty;
            vm.InspectionQty = InspectionQty;
            vm.MultiplierGT = MultiplierGT;
            vm.MultiplierLT = MultiplierLT;
            vm.Sample = Sample;
            return PartialView("_Filters", vm);
        }


        public JsonResult GetPendingJobReceivesHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var Records = new JobReceiveQALineService(db, _unitOfWork).GetPendingReceiveHelpList(filter, searchTerm);

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

        public JsonResult GetPendingJobWorkerHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var Records = new JobReceiveQALineService(db, _unitOfWork).GetPendingJobWorkerHelpList(filter, searchTerm);

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

        public JsonResult GetPendingProductHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var Records = new JobReceiveQALineService(db,_unitOfWork).GetPendingProductHelpList(filter, searchTerm);

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
