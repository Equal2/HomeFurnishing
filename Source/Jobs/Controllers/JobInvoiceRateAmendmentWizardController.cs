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
using JobInvoiceAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;



namespace Jobs.Controllers
{
    [Authorize]
    public class JobInvoiceRateAmendmentWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobInvoiceAmendmentHeaderService _JobInvoiceAmendmentHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public JobInvoiceRateAmendmentWizardController(IJobInvoiceAmendmentHeaderService JobInvoiceAmendmentHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceAmendmentHeaderService = JobInvoiceAmendmentHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!JobInvoiceAmendmentEvents.Initialized)
            {
                JobInvoiceAmendmentEvents Obj = new JobInvoiceAmendmentEvents();
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
            JobInvoiceAmendmentHeaderViewModel vm = new JobInvoiceAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null)
            {
                return RedirectToAction("CreateJobInvoiceAmendment", "JobInvoiceSettings", new { id = id }).Warning("Please create job amendment settings");
            }
            ViewBag.ProcessId = settings.ProcessId;
            return View();
        }

        public ActionResult AjaxGetJsonData(int ProcessId, int DocType, DateTime? FromDate, DateTime? ToDate,
            string JobInvoiceHeaderId, string JobWorkerId, string ProductId, string Dimension1Id, string Dimension2Id,
            string ProductGroupId, string ProductCategoryId, decimal? Rate, decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
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
            var data = FilterData(ProcessId, DocType, FromDate, ToDate, JobInvoiceHeaderId, JobWorkerId,
                                            ProductId, Dimension1Id, Dimension2Id, ProductGroupId, ProductCategoryId, Rate, NewRate, MultiplierGT, MultiplierLT, Sample);

            var RecCount = data.Count();

            if (RecCount > 1000 || RecCount == 0)
            {
                Success = false;
                return Json(new { Success = Success, Message = (RecCount > 1000 ? "No of records exceeding 1000." : "No Records found.") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CList = data.ToList().Select(m => new JobInvoiceAmendmentWizardViewModel
                {
                    SInvoiceDate = m.InvoiceDate.ToString("dd/MMM/yyyy"),
                    JobInvoiceLineId = m.JobInvoiceLineId,
                    InvoiceNo = m.InvoiceNo,
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
            public List<JobInvoiceAmendmentWizardViewModel> data { get; set; }
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
        private IQueryable<JobInvoiceAmendmentWizardViewModel> FilterData(int ProcessId, int DocType, DateTime? FromDate, DateTime? ToDate,
                                                                    string JobInvoiceHeaderId, string JobWorkerId, string ProductId, string Dimension1Id,
            string Dimension2Id, string ProductGroupId, string ProductCategoryId,
            decimal? Rate, decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {

            List<int> JobInvoiceHeaderIds = new List<int>();
            if (!string.IsNullOrEmpty(JobInvoiceHeaderId))
                foreach (var item in JobInvoiceHeaderId.Split(','))
                    JobInvoiceHeaderIds.Add(Convert.ToInt32(item));


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

            List<int> ProductGroupIds = new List<int>();
            if (!string.IsNullOrEmpty(ProductGroupId))
                foreach (var item in ProductGroupId.Split(','))
                    ProductGroupIds.Add(Convert.ToInt32(item));

            //List<int> ProductCategoryIds = new List<int>();
            //if (!string.IsNullOrEmpty(ProductCategoryId))
            //    foreach (var item in ProductCategoryId.Split(','))
            //        ProductCategoryIds.Add(Convert.ToInt32(item));



            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(DocType, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(Settings.filterContraDocTypes)) { contraDocTypes = Settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }


            IQueryable<JobInvoiceAmendmentWizardViewModel> _data = from p in db.ViewJobInvoiceBalanceForRateAmendment
                                                                   join t in db.JobInvoiceLine on p.JobInvoiceLineId equals t.JobInvoiceLineId
                                                                   join ih in db.JobInvoiceHeader on p.JobInvoiceHeaderId equals ih.JobInvoiceHeaderId
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
                                                                   where ih.ProcessId == ProcessId
                                                                   && (string.IsNullOrEmpty(Settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(t.JobInvoiceHeader.DocTypeId.ToString()))
                                                                   && ih.SiteId == SiteId && ih.DivisionId == DivisionId && p.Rate != NewRate
                                                                   select new JobInvoiceAmendmentWizardViewModel
                                                                {
                                                                    InvoiceDate = p.InvoiceDate,
                                                                    InvoiceNo = p.JobInvoiceNo,
                                                                    JobInvoiceLineId = p.JobInvoiceLineId,
                                                                    OldRate = p.Rate,
                                                                    Rate = NewRate,
                                                                    JobWorkerName = jwtab.Name + "," + jwtab.Suffix,
                                                                    ProductName = prodtab.ProductName,
                                                                    Dimension1Name = dimtab.Dimension1Name,
                                                                    Dimension2Name = dim2tab.Dimension2Name,
                                                                    JobInvoiceHeaderId = p.JobInvoiceHeaderId,
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
                _data = _data.Where(m => m.InvoiceDate >= FromDate);

            if (ToDate.HasValue)
                _data = _data.Where(m => m.InvoiceDate <= ToDate);

            if (Rate.HasValue)
                _data = _data.Where(m => m.OldRate == Rate.Value);

            if (MultiplierGT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier >= MultiplierGT.Value);

            if (MultiplierLT.HasValue)
                _data = _data.Where(m => m.UnitConversionMultiplier <= MultiplierLT.Value);

            if (!string.IsNullOrEmpty(JobInvoiceHeaderId))
                _data = _data.Where(m => JobInvoiceHeaderIds.Contains(m.JobInvoiceHeaderId));

            if (!string.IsNullOrEmpty(JobWorkerId))
                _data = _data.Where(m => JobWorkerIds.Contains(m.JobWorkerId));

            if (!string.IsNullOrEmpty(ProductId))
                _data = _data.Where(m => m.ProductName.Contains(ProductId));

            if (!string.IsNullOrEmpty(Dimension1Id))
                _data = _data.Where(m => Dimension1Ids.Contains(m.Dimension1Id ?? 0));

            if (!string.IsNullOrEmpty(Dimension2Id))
                _data = _data.Where(m => Dimension2Ids.Contains(m.Dimension2Id ?? 0));

            if (!string.IsNullOrEmpty(ProductGroupId))
                _data = _data.Where(m => ProductGroupIds.Contains(m.ProductGroupId));           

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
            _data = _data.OrderBy(m => m.InvoiceDate).ThenBy(m => m.InvoiceNo);
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
            return _data.Select(m => new JobInvoiceAmendmentWizardViewModel
            {
                InvoiceDate = m.InvoiceDate,
                InvoiceNo = m.InvoiceNo,
                JobInvoiceLineId = m.JobInvoiceLineId,
                OldRate = m.OldRate,
                Rate = m.Rate,
                JobWorkerName = m.JobWorkerName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                JobInvoiceHeaderId = m.JobInvoiceHeaderId,
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

        public ActionResult ConfirmedJobInvoices(List<JobInvoiceAmendmentWizardViewModel> ConfirmedList, int DocTypeId)
        {
            System.Web.HttpContext.Current.Session["RateAmendmentWizardOrders"] = ConfirmedList;
            return Json(new { Success = "URL", Data = "/JobInvoiceRateAmendmentWizard/Create/" + DocTypeId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SelectedRecords(List<JobInvoiceAmendmentWizardViewModel> SelectedRecords)
        {
            var OrderIds = SelectedRecords.Select(m => m.JobInvoiceLineId).ToArray();
            var RecordDetails = (from p in db.ViewJobInvoiceBalanceForRateAmendment
                                 join t in db.JobInvoiceLine on p.JobInvoiceLineId equals t.JobInvoiceLineId
                                 where OrderIds.Contains(p.JobInvoiceLineId)
                                 select new JobInvoiceAmendmentWizardViewModel
                                 {
                                     InvoiceDate = p.InvoiceDate,
                                     InvoiceNo = p.JobInvoiceNo,
                                     JobInvoiceLineId = p.JobInvoiceLineId,
                                     OldRate = p.Rate,
                                     JobWorkerName = p.JobWorker.Person.Name,
                                     ProductName = p.Product.ProductName,
                                     Dimension1Name = p.Dimension1.Dimension1Name,
                                     Dimension2Name = p.Dimension2.Dimension2Name,
                                     ProductGroupName = p.Product.ProductGroup.ProductGroupName

                                 }).ToList();

            var RecordDetailList = RecordDetails.Select(m => new JobInvoiceAmendmentWizardViewModel
            {
                SInvoiceDate = m.InvoiceDate.ToString("dd/MMM/yyyy"),
                JobInvoiceLineId = m.JobInvoiceLineId,
                InvoiceNo = m.InvoiceNo,
                JobWorkerName = m.JobWorkerName,
                ProductName = m.ProductName,
                Dimension1Name = m.Dimension1Name,
                Dimension2Name = m.Dimension2Name,
                OldRate = m.OldRate,
                Rate = SelectedRecords.Where(t => t.JobInvoiceLineId == m.JobInvoiceLineId).FirstOrDefault().Rate,
                ProductGroupName = m.ProductGroupName
            }).ToList();

            return PartialView("_SelectedRecords", RecordDetailList);

        }




        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobInvoiceAmendmentHeaderViewModel vm = new JobInvoiceAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);
            vm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.ProcessId = settings.ProcessId;
            var EmpId = new EmployeeService(_unitOfWork).GetEmloyeeForUser(User.Identity.GetUserId());
            if (EmpId.HasValue && EmpId.Value != 0)
                vm.OrderById = EmpId.Value;

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobInvoiceAmendmentHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobInvoiceAmendmentHeaderViewModel vm)
        {

            int Serial = 1;
            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;
            int pk = 0;

            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            var SelectedJobInvoices = (List<JobInvoiceAmendmentWizardViewModel>)System.Web.HttpContext.Current.Session["RateAmendmentWizardOrders"];

            JobInvoiceAmendmentHeader Header = AutoMapper.Mapper.Map<JobInvoiceAmendmentHeaderViewModel, JobInvoiceAmendmentHeader>(vm);

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();

            List<LineReferenceIds> RefIds = new List<LineReferenceIds>();
            bool HeaderChargeEdit = false;
            int PersonCount = 0;

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            //int? CalculationId = settings.CalculationId;


            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            if (vm.JobInvoiceSettings.isVisibleHeaderJobWorker && !vm.JobWorkerId.HasValue)
            {
                ModelState.AddModelError("JobWorkerId", "The JobWorker field is required");
            }

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceAmendmentDocEvents.beforeWizardSaveEvent(this, new JobEventArgs(vm.JobInvoiceAmendmentHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }
            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before save";
            #endregion

            #region DocTypeTimeLineValidation

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXC"] += ExceptionMsg;

            #endregion

            

            if (ModelState.IsValid && BeforeSave && !EventException && (TimePlanValidation || Continue))
            {

                if (SelectedJobInvoices.Count > 0)
                {
                    Header.Status = (int)StatusConstants.Drafted;
                    Header.CreatedDate = DateTime.Now;
                    Header.ModifiedDate = DateTime.Now;
                    Header.CreatedBy = User.Identity.Name;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceAmendmentHeader.Add(Header);
                    //_JobInvoiceAmendmentHeaderService.Create(pt);                

                    var JobInvoiceLineIds = SelectedJobInvoices.Select(m => m.JobInvoiceLineId).ToArray();

                    var JobInvoiceBalanceRecords = (from p in db.ViewJobInvoiceBalanceForRateAmendment
                                                    where JobInvoiceLineIds.Contains(p.JobInvoiceLineId)
                                                    select p).AsNoTracking().ToList();

                    var JobInvoiceRecords = (from p in db.JobInvoiceLine
                                             where JobInvoiceLineIds.Contains(p.JobInvoiceLineId)
                                             select p).AsNoTracking().ToList();



                    int? CalculationId = 0;
                    int JobInvoiceLineId = JobInvoiceBalanceRecords.FirstOrDefault().JobInvoiceLineId;
                    var SalesTaxGroupPerson = (from L in db.JobInvoiceLine where L.JobInvoiceLineId == JobInvoiceLineId select new { SalesTaxGroupPersonId = (int?)L.JobInvoiceHeader.SalesTaxGroupPerson.ChargeGroupPersonId ?? 0 }).FirstOrDefault();
                    if (SalesTaxGroupPerson != null)
                    {
                        CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(Header.DocTypeId, SalesTaxGroupPerson.SalesTaxGroupPersonId, Header.SiteId, Header.DivisionId) ?? 0;
                    }
                    if (CalculationId == 0)
                        CalculationId = settings.CalculationId ?? 0;


                    foreach (var item in SelectedJobInvoices)
                    {

                        


                        if (item.Rate - JobInvoiceBalanceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().Rate != 0)
                        {
                            JobInvoiceRateAmendmentLine line = new JobInvoiceRateAmendmentLine();

                            line.JobInvoiceAmendmentHeaderId = Header.JobInvoiceAmendmentHeaderId;
                            line.JobInvoiceLineId = item.JobInvoiceLineId;
                            line.Qty = JobInvoiceBalanceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().BalanceQty;
                            line.AmendedRate = item.Rate;
                            line.Rate = line.AmendedRate - JobInvoiceBalanceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().Rate;
                            line.Amount = JobInvoiceBalanceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().BalanceQty * JobInvoiceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().UnitConversionMultiplier * line.Rate;
                            line.Amount = DecimalRoundOff.amountToFixed(line.Amount, settings.AmountRoundOff);
                            line.JobInvoiceRate = JobInvoiceBalanceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().Rate;
                            line.JobWorkerId = JobInvoiceBalanceRecords.Where(m => m.JobInvoiceLineId == item.JobInvoiceLineId).FirstOrDefault().JobWorkerId;
                            line.Sr = Serial++;
                            line.JobInvoiceRateAmendmentLineId = pk;
                            line.CreatedDate = DateTime.Now;
                            line.ModifiedDate = DateTime.Now;
                            line.CreatedBy = User.Identity.Name;
                            line.ModifiedBy = User.Identity.Name;
                            LineStatus.Add(line.JobInvoiceLineId, line.Rate);
                            RefIds.Add(new LineReferenceIds { LineId = line.JobInvoiceRateAmendmentLineId, RefLineId = line.JobInvoiceLineId });

                            line.ObjectState = Model.ObjectState.Added;
                            db.JobInvoiceRateAmendmentLine.Add(line);

                            LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobInvoiceRateAmendmentLineId, HeaderTableId = Header.JobInvoiceAmendmentHeaderId, PersonID = line.JobWorkerId, DealQty = 0 });

                            pk++;

                        }
                    }

                    //new JobInvoiceLineStatusService(_unitOfWork).UpdateJobRateOnAmendmentMultiple(LineStatus, pt.DocDate, ref db);


                    int[] RecLineIds = null;
                    RecLineIds = RefIds.Select(m => m.RefLineId).ToArray();

                    var Charges = (from p in db.JobInvoiceLine
                                   where RecLineIds.Contains(p.JobInvoiceLineId)
                                   join LineCharge in db.JobInvoiceLineCharge on p.JobInvoiceLineId equals LineCharge.LineTableId
                                   join HeaderCharge in db.JobInvoiceHeaderCharges on p.JobInvoiceHeaderId equals HeaderCharge.HeaderTableId
                                   group new { p, LineCharge, HeaderCharge } by new { p.JobInvoiceLineId } into g
                                   select new
                                   {
                                       LineId = g.Key.JobInvoiceLineId,
                                       HeaderCharges = g.Select(m => m.HeaderCharge).ToList(),
                                       Linecharges = g.Select(m => m.LineCharge).ToList(),
                                   }).ToList();



                    var LineListWithReferences = (from p in LineList
                                                  join t in RefIds on p.LineTableId equals t.LineId
                                                  join t2 in Charges on t.RefLineId equals t2.LineId into table
                                                  from LineLis in table.DefaultIfEmpty()
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
                                                      RLineCharges = (LineLis == null ? null : Mapper.Map<List<LineChargeViewModel>>(LineLis.Linecharges)),
                                                  }).ToList();


                    if (CalculationId != null)
                        new ChargesCalculationService(_unitOfWork).CalculateCharges(LineListWithReferences, Header.JobInvoiceAmendmentHeaderId, (int)CalculationId, 0, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceAmendmentHeaderCharges", "Web.JobInvoiceRateAmendmentLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                    //Saving Charges
                    foreach (var item in LineCharges)
                    {

                        JobInvoiceRateAmendmentLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobInvoiceRateAmendmentLineCharge>(item);
                        PoLineCharge.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceRateAmendmentLineCharge.Add(PoLineCharge);
                    }


                    //Saving Header charges
                    for (int i = 0; i < HeaderCharges.Count(); i++)
                    {

                        if (!HeaderChargeEdit)
                        {
                            JobInvoiceAmendmentHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobInvoiceAmendmentHeaderCharge>(HeaderCharges[i]);
                            POHeaderCharge.HeaderTableId = Header.JobInvoiceAmendmentHeaderId;
                            POHeaderCharge.PersonID = Header.JobWorkerId;
                            POHeaderCharge.ObjectState = Model.ObjectState.Added;
                            db.JobInvoiceAmendmentHeaderCharge.Add(POHeaderCharge);
                        }
                        else
                        {
                            var footercharge = new JobInvoiceAmendmentHeaderChargeService(db).Find(HeaderCharges[i].Id);
                            footercharge.Rate = HeaderCharges[i].Rate;
                            footercharge.Amount = HeaderCharges[i].Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceAmendmentHeaderCharge.Add(footercharge);
                        }

                    }



                    try
                    {
                        JobInvoiceAmendmentDocEvents.onWizardSaveEvent(this, new JobEventArgs(Header.JobInvoiceAmendmentHeaderId, EventModeConstants.Add), ref db);
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
                        JobInvoiceAmendmentDocEvents.afterWizardSaveEvent(this, new JobEventArgs(Header.JobInvoiceAmendmentHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.JobInvoiceAmendmentHeaderId,
                        ActivityType = (int)ActivityTypeContants.WizardCreate,
                        DocNo = Header.DocNo,
                        DocDate = Header.DocDate,
                        DocStatus = Header.Status,
                    }));


                    Session.Remove("RateAmendmentWizardOrders");

                }
                return RedirectToAction("Index", "JobInvoiceAmendmentHeader", new { id = Header.DocTypeId }).Success("Data saved Successfully");


            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        public ActionResult Filters(int DocTypeId, int ProcessId, DateTime? FromDate, DateTime? ToDate, string JobInvoiceHeaderId, string JobWorkerId,
            string ProductId, string Dimension1Id, string Dimension2Id, string ProductGroupId, string ProductCategoryId, decimal? Rate,
            decimal NewRate, decimal? MultiplierGT, decimal? MultiplierLT, string Sample)
        {
            JobInvoiceRateAmendmentWizardFilterViewModel vm = new JobInvoiceRateAmendmentWizardFilterViewModel();

            List<SelectListItem> tempSOD = new List<SelectListItem>();
            tempSOD.Add(new SelectListItem { Text = "Include Sample", Value = "Include" });
            tempSOD.Add(new SelectListItem { Text = "Exculde Sample", Value = "Exculde" });
            tempSOD.Add(new SelectListItem { Text = "Only Sample", Value = "Only" });

            ViewBag.SOD = new SelectList(tempSOD, "Value", "Text", Sample);

            vm.DocTypeId = DocTypeId;
            vm.ProcessId = ProcessId;
            vm.FromDate = FromDate;
            vm.ToDate = ToDate;
            vm.JobInvoiceHeaderId = JobInvoiceHeaderId;
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


        public JsonResult GetPendingJobInvoicesHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var Records = new JobInvoiceRateAmendmentLineService(_unitOfWork).GetPendingJobInvoiceHelpList(filter, searchTerm);

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
            var Records = new JobInvoiceRateAmendmentLineService(_unitOfWork).GetPendingJobWorkerHelpList(filter, searchTerm);

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
            var Records = new JobInvoiceRateAmendmentLineService(_unitOfWork).GetPendingProductHelpList(filter, searchTerm);

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
