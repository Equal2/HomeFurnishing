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
using JobOrderAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;


namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderRateAmendmentWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobOrderAmendmentHeaderService _JobOrderAmendmentHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobOrderRateAmendmentWizardController(IJobOrderAmendmentHeaderService JobOrderAmendmentHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderAmendmentHeaderService = JobOrderAmendmentHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!JobOrderAmendmentEvents.Initialized)
            {
                JobOrderAmendmentEvents Obj = new JobOrderAmendmentEvents();
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
                    return RedirectToAction("RateAmendtmentWizard", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public ActionResult RateAmendtmentWizard(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobOrderAmendmentHeaderViewModel vm = new JobOrderAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null)
            {
                return RedirectToAction("CreateJobOrderAmendment", "JobOrderSettings", new { id = id }).Warning("Please create job amendment settings");
            }
            ViewBag.ProcessId = settings.ProcessId;
            return View();
        }

        public JsonResult AjaxGetJsonData(int ProcessId, int DocType, DateTime? FromDate, DateTime? ToDate, string JobOrderHeaderId, string JobWorkerId
            , string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? Rate, decimal NewRate
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

            var data = FilterData(ProcessId, DocType, FromDate, ToDate, JobOrderHeaderId, JobWorkerId,
                                            ProductId, Dimension1Id, Dimension2Id, ProductGroupId, ProductCategoryId, Rate, NewRate, MultiplierGT, MultiplierLT, Sample);

            var RecCount = data.Count();

            if (RecCount > 1000 || RecCount == 0)
            {
                Success = false;
                return Json(new { Success = Success, Message = (RecCount > 1000 ? "No of records exceeding 1000." : "No Records found.") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CList = data.ToList().Select(m => new JobOrderAmendmentWizardViewModel
                {
                    SOrderDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                    JobOrderLineId = m.JobOrderLineId,
                    OrderNo = m.OrderNo,
                    JobWorkerName = m.JobWorkerName,
                    ProductName = m.ProductName,
                    Dimension1Name = m.Dimension1Name,
                    Dimension2Name = m.Dimension2Name,
                    OldRate = m.OldRate,
                    Rate = m.Rate,
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
            public List<JobOrderAmendmentWizardViewModel> data { get; set; }
        }

        // here we simulate data from a database table.
        // !!!!DO NOT DO THIS IN REAL APPLICATION !!!!
        //private static List<DataItem> CreateData()
        //{
        //    Random rnd = new Random();
        //    List<DataItem> list = new List<DataItem>();
        //    for (int i = 1; i <= TOTAL_ROWS; i++)
        //    {
        //        DataItem item = new DataItem();
        //        item.Name = "Name_" + i.ToString().PadLeft(5, '0');
        //        DateTime dob = new DateTime(1900 + rnd.Next(1, 100), rnd.Next(1, 13), rnd.Next(1, 28));
        //        item.Age = ((DateTime.Now - dob).Days / 365).ToString();
        //        item.DoB = dob.ToShortDateString();
        //        list.Add(item);
        //    }
        //    return list;
        //}

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
        private IQueryable<JobOrderAmendmentWizardViewModel> FilterData(int ProcessId, int DocType, DateTime? FromDate, DateTime? ToDate,
                                                                    string JobOrderHeaderId, string JobWorkerId, string ProductId, string Dimension1Id,
            string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? Rate, decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {

            List<int> JobOrderHeaderIds = new List<int>();
            if (!string.IsNullOrEmpty(JobOrderHeaderId))
                foreach (var item in JobOrderHeaderId.Split(','))
                    JobOrderHeaderIds.Add(Convert.ToInt32(item));


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

            var Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(DocType, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            IQueryable<JobOrderAmendmentWizardViewModel> _data = from p in db.ViewJobOrderBalanceForInvoice
                                                                 join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                                                 join jw in db.Persons on p.JobWorkerId equals jw.PersonID into jwtable
                                                                 from jwtab in jwtable.DefaultIfEmpty()
                                                                 join prod in db.FinishedProduct on p.ProductId equals prod.ProductId into prodtable
                                                                 from prodtab in prodtable.DefaultIfEmpty()
                                                                 join dim1 in db.Dimension1 on p.Dimension1Id equals dim1.Dimension1Id into dimtable
                                                                 from dimtab in dimtable.DefaultIfEmpty()
                                                                 join dim2 in db.Dimension2 on p.Dimension2Id equals dim2.Dimension2Id into dim2table
                                                                 from dim2tab in dim2table.DefaultIfEmpty()
                                                                 join pg in db.ProductGroups on prodtab.ProductGroupId equals pg.ProductGroupId into pgtable
                                                                 from pgtab in pgtable.DefaultIfEmpty()
                                                                 join pc in db.ProductCategory on prodtab.ProductCategoryId equals pc.ProductCategoryId into pctable
                                                                 from pctab in pctable.DefaultIfEmpty()
                                                                 where t.JobOrderHeader.ProcessId == ProcessId && p.BalanceQty > 0
                                                                 && (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.JobOrderHeader.DocTypeId.ToString()))
                                                                 //&& (FromDate.HasValue ? p.OrderDate >= FromDate : 1 == 1)
                                                                 //&& (ToDate.HasValue ? p.OrderDate <= ToDate : 1 == 1)
                                                                 //&& (Rate.HasValue ? p.Rate == Rate : 1 == 1)
                                                                 //&& (string.IsNullOrEmpty(JobOrderHeaderId) ? 1 == 1 : JobOrderHeaderIds.Contains(p.JobOrderHeaderId))
                                                                 //&& (string.IsNullOrEmpty(JobWorkerId) ? 1 == 1 : JobWorkerIds.Contains(p.JobWorkerId))
                                                                 //&& (string.IsNullOrEmpty(ProductId) ? 1 == 1 : ProductIds.Contains(p.ProductId))
                                                                 //&& (string.IsNullOrEmpty(Dimension1Id) ? 1 == 1 : Dimension1Ids.Contains(p.Dimension1Id ?? 0))
                                                                 //&& (string.IsNullOrEmpty(Dimension1Id) ? 1 == 1 : Dimension1Ids.Contains(p.Dimension1Id ?? 0))
                                                                 //&& (string.IsNullOrEmpty(ProductGroupId) ? 1 == 1 : ProductGroupIds.Contains(prodtab.ProductGroupId ?? 0))
                                                                 select new JobOrderAmendmentWizardViewModel
                                                              {
                                                                  OrderDate = p.OrderDate,
                                                                  OrderNo = p.JobOrderNo,
                                                                  JobOrderLineId = p.JobOrderLineId,
                                                                  OldRate = p.Rate,
                                                                  Rate = NewRate,
                                                                  JobWorkerName = jwtab.Name,
                                                                  ProductName = prodtab.ProductName,
                                                                  Dimension1Name = dimtab.Dimension1Name,
                                                                  Dimension2Name = dim2tab.Dimension2Name,
                                                                  JobOrderHeaderId = p.JobOrderHeaderId,
                                                                  JobWorkerId = p.JobWorkerId,
                                                                  ProductGroupId = pgtab.ProductGroupId,
                                                                  ProductGroupName = pgtab.ProductGroupName,
                                                                  ProductCategoryId = pctab.ProductCategoryId,
                                                                  ProductCategoryName = pctab.ProductCategoryName,
                                                                  ProdId = p.ProductId,
                                                                  Dimension1Id = p.Dimension1Id,
                                                                  Dimension2Id = p.Dimension2Id,
                                                                  UnitConversionMultiplier = t.UnitConversionMultiplier,
                                                                  Sample = prodtab.IsSample,
                                                              };



            //if (FromDate.HasValue)
            //    _data = from p in _data
            //            where p.OrderDate >= FromDate
            //            select p;

            if (FromDate.HasValue)
                _data = _data.Where(m => m.OrderDate >= FromDate);

            if (ToDate.HasValue)
                _data = _data.Where(m => m.OrderDate <= ToDate);

            if (Rate.HasValue && Rate.Value > 0)
                _data = _data.Where(m => m.OldRate == Rate.Value);

            if (MultiplierGT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier >= MultiplierGT.Value);

            if (MultiplierLT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier <= MultiplierLT.Value);


            if (!string.IsNullOrEmpty(JobOrderHeaderId))
                _data = _data.Where(m => JobOrderHeaderIds.Contains(m.JobOrderHeaderId));

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

            if (!string.IsNullOrEmpty(Sample) && Sample != "Include")
            {
                if (Sample == "Exclude")
                    _data = _data.Where(m => m.Sample == false);
                else if (Sample == "Only")
                    _data = _data.Where(m => m.Sample == true);
            }

            // simulate sort
            //if (sortColumn == 0)
            //{// sort Name
            _data = _data.OrderBy(m => m.OrderDate).ThenBy(m => m.OrderNo);
            //}
            //else if (sortColumn == 1)
            //{// sort Age
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DocNo) : _data.OrderByDescending(m => m.DocNo);
            //}
            //else if (sortColumn == 2)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DocDate) : _data.OrderByDescending(m => m.DocDate);
            //}
            //else if (sortColumn == 3)
            //{   // sort DoB
            //    _data = sortDirection == "asc" ? _data.OrderBy(m => m.DueDate) : _data.OrderByDescending(m => m.DueDate);
            //}

            // get just one page of data
            return _data.Select(m => new JobOrderAmendmentWizardViewModel
            {
                OrderDate = m.OrderDate,
                OrderNo = m.OrderNo,
                JobOrderLineId = m.JobOrderLineId,
                OldRate = m.OldRate,
                Rate = m.Rate,
                JobWorkerName = m.JobWorkerName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                JobOrderHeaderId = m.JobOrderHeaderId,
                JobWorkerId = m.JobWorkerId,
                ProductGroupId = m.ProductGroupId,
                ProductGroupName = m.ProductGroupName,
                ProductCategoryId = m.ProductCategoryId,
                ProductCategoryName = m.ProductCategoryName,
                ProdId = m.ProdId,
                Dimension1Id = m.Dimension1Id,
                Dimension2Id = m.Dimension2Id,
                UnitConversionMultiplier = m.UnitConversionMultiplier,
                Sample = m.Sample,
            });

        }

        public ActionResult ConfirmedJobOrders(List<JobOrderAmendmentWizardViewModel> ConfirmedList, int DocTypeId)
        {
            System.Web.HttpContext.Current.Session["RateAmendmentWizardOrders"] = ConfirmedList;
            return Json(new { Success = "URL", Data = "/JobOrderRateAmendmentWizard/Create/" + DocTypeId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SelectedRecords(List<JobOrderAmendmentWizardViewModel> SelectedRecords)
        {
            var OrderIds = SelectedRecords.Select(m => m.JobOrderLineId).ToArray();
            var RecordDetails = (from p in db.ViewJobOrderBalanceForInvoice
                                 join t in db.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                 where OrderIds.Contains(p.JobOrderLineId)
                                 select new JobOrderAmendmentWizardViewModel
                                 {
                                     OrderDate = p.OrderDate,
                                     OrderNo = p.JobOrderNo,
                                     JobOrderLineId = p.JobOrderLineId,
                                     OldRate = p.Rate,
                                     JobWorkerName = p.JobWorker.Person.Name,
                                     ProductName = p.Product.ProductName,
                                     Dimension1Name = p.Dimension1.Dimension1Name,
                                     Dimension2Name = p.Dimension2.Dimension2Name,
                                     ProductGroupName = p.Product.ProductGroup.ProductGroupName

                                 }).ToList();

            var RecordDetailList = RecordDetails.Select(m => new JobOrderAmendmentWizardViewModel
            {
                SOrderDate = m.OrderDate.ToString("dd/MMM/yyyy"),
                JobOrderLineId = m.JobOrderLineId,
                OrderNo = m.OrderNo,
                JobWorkerName = m.JobWorkerName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                OldRate = m.OldRate,
                Rate = SelectedRecords.Where(t => t.JobOrderLineId == m.JobOrderLineId).FirstOrDefault().Rate,
                ProductGroupName = m.ProductGroupName
            }).ToList();

            return PartialView("_SelectedRecords", RecordDetailList);

        }




        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobOrderAmendmentHeaderViewModel vm = new JobOrderAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(id, vm.DivisionId, vm.SiteId);
            vm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.ProcessId = settings.ProcessId;
            var EmpId = new EmployeeService(_unitOfWork).GetEmloyeeForUser(User.Identity.GetUserId());
            if (EmpId.HasValue && EmpId.Value != 0)
                vm.OrderById = EmpId.Value;

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobOrderAmendmentHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderAmendmentHeaderViewModel vm)
        {
            bool BeforeSave = true;
            int Serial = 1;
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            JobOrderAmendmentHeader pt = AutoMapper.Mapper.Map<JobOrderAmendmentHeaderViewModel, JobOrderAmendmentHeader>(vm);

            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (!vm.JobOrderSettings.isVisibleJobWorkerLine && !vm.JobWorkerId.HasValue)
            {
                ModelState.AddModelError("JobWorkerId", "The JobWorker field is required");
            }

            try
            {
                BeforeSave = JobOrderAmendmentDocEvents.beforeWizardSaveEvent(this, new JobEventArgs(vm.JobOrderAmendmentHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }


            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before save";

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                pt.Status = (int)StatusConstants.Drafted;
                pt.CreatedDate = DateTime.Now;
                pt.ModifiedDate = DateTime.Now;
                pt.CreatedBy = User.Identity.Name;
                pt.ModifiedBy = User.Identity.Name;
                pt.ObjectState = Model.ObjectState.Added;
                db.JobOrderAmendmentHeader.Add(pt);
                //_JobOrderAmendmentHeaderService.Create(pt);


                var SelectedJobOrders = (List<JobOrderAmendmentWizardViewModel>)System.Web.HttpContext.Current.Session["RateAmendmentWizardOrders"];

                var JobOrderLineIds = SelectedJobOrders.Select(m => m.JobOrderLineId).ToArray();

                var JobOrderBalanceRecords = (from p in db.ViewJobOrderBalanceForInvoice
                                              where JobOrderLineIds.Contains(p.JobOrderLineId)
                                              select p).AsNoTracking().ToList();

                var JobOrderRecords = (from p in db.JobOrderLine
                                       where JobOrderLineIds.Contains(p.JobOrderLineId)
                                       select p).AsNoTracking().ToList();

                foreach (var item in SelectedJobOrders)
                {

                    if (item.Rate - JobOrderBalanceRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().Rate != 0)
                    {
                        JobOrderRateAmendmentLine line = new JobOrderRateAmendmentLine();

                        line.JobOrderAmendmentHeaderId = pt.JobOrderAmendmentHeaderId;
                        line.JobOrderLineId = item.JobOrderLineId;
                        line.Qty = JobOrderBalanceRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().BalanceQty;
                        line.AmendedRate = item.Rate;
                        line.Rate = line.AmendedRate - JobOrderBalanceRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().Rate;
                        line.Amount = JobOrderBalanceRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().BalanceQty * JobOrderRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().UnitConversionMultiplier * line.Rate;
                        line.Amount = DecimalRoundOff.amountToFixed(line.Amount, settings.AmountRoundOff);
                        line.JobOrderRate = JobOrderBalanceRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().Rate;
                        line.JobWorkerId = JobOrderBalanceRecords.Where(m => m.JobOrderLineId == item.JobOrderLineId).FirstOrDefault().JobWorkerId;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        LineStatus.Add(line.JobOrderLineId, line.Rate);

                        line.ObjectState = Model.ObjectState.Added;
                        db.JobOrderRateAmendmentLine.Add(line);

                    }
                }

                new JobOrderLineStatusService(_unitOfWork).UpdateJobRateOnAmendmentMultiple(LineStatus, pt.DocDate, ref db);


                try
                {
                    JobOrderAmendmentDocEvents.onWizardSaveEvent(this, new JobEventArgs(pt.JobOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
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
                    TempData["CSEXC"] += message;
                    PrepareViewBag(vm.DocTypeId);
                    ViewBag.Mode = "Add";
                    return View("Create", vm);
                }

                try
                {
                    JobOrderAmendmentDocEvents.afterWizardSaveEvent(this, new JobEventArgs(pt.JobOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobOrderAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.WizardCreate,
                    User = User.Identity.Name,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate
                });


                Session.Remove("RateAmendmentWizardOrders");


                return RedirectToAction("Index", "JobOrderAmendmentHeader", new { id = pt.DocTypeId }).Success("Data saved Successfully");


            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        public ActionResult Filters(int DocTypeId, int ProcessId, DateTime? FromDate, DateTime? ToDate,
            string JobOrderHeaderId, string JobWorkerId, string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId,
            string ProductCategoryId, decimal? Rate, decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {
            JobOrderRateAmendmentWizardFilterViewModel vm = new JobOrderRateAmendmentWizardFilterViewModel();

            List<SelectListItem> tempSOD = new List<SelectListItem>();
            tempSOD.Add(new SelectListItem { Text = "Include Sample", Value = "Include" });
            tempSOD.Add(new SelectListItem { Text = "Exculde Sample", Value = "Exculde" });
            tempSOD.Add(new SelectListItem { Text = "Only Sample", Value = "Only" });

            ViewBag.SOD = new SelectList(tempSOD, "Value", "Text", Sample);


            vm.DocTypeId = DocTypeId;
            vm.ProcessId = ProcessId;
            vm.FromDate = FromDate;
            vm.ToDate = ToDate;
            vm.JobOrderHeaderId = JobOrderHeaderId;
            vm.JobWorkerId = JobWorkerId;
            vm.ProductId = ProductId;
            vm.Dimension1Id = Dimension1Id;
            vm.Dimension2Id = Dimension2Id;
            vm.ProductGroupId = ProductGroupId;
            vm.ProductCategoryId = ProductCategoryId;
            vm.Rate = Rate;
            vm.NewRate = NewRate;
            vm.MultiplierGT = MultiplierGT;
            vm.MultiplierLT = MultiplierLT;
            vm.Sample = Sample;
            return PartialView("_Filters", vm);
        }


        public JsonResult GetPendingJobOrdersHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var Records = new JobOrderRateAmendmentLineService(_unitOfWork).GetPendingJobOrderHelpList(filter, searchTerm);

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
            var Records = new JobOrderRateAmendmentLineService(_unitOfWork).GetPendingJobWorkerHelpList(filter, searchTerm);

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
            var Records = new JobOrderRateAmendmentLineService(_unitOfWork).GetPendingProductHelpList(filter, searchTerm);

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
