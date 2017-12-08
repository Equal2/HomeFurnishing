using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class RateListHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IRateListHeaderService _RateListHeaderService;
        IRateListPersonRateGroupService _RateListPersonRateGroupService;
        IRateListProductRateGroupService _RateListProductRateGroupService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public RateListHeaderController(IRateListHeaderService RateListHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IRateListPersonRateGroupService PRG, IRateListProductRateGroupService PRRG)
        {
            _RateListHeaderService = RateListHeaderService;
            _RateListPersonRateGroupService = PRG;
            _RateListProductRateGroupService = PRRG;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/

        public ActionResult Index()
        {
            var RateListHeader = _RateListHeaderService.GetRateListHeaderList();
            return View(RateListHeader);
        }

        public void PrepareViewBag()
        {
            List<SelectListItem> AddList = new List<SelectListItem>();
            AddList.Add(new SelectListItem { Text = RateListCalculateOnConstants.Qty, Value = RateListCalculateOnConstants.Qty });
            AddList.Add(new SelectListItem { Text = RateListCalculateOnConstants.DealQty, Value = RateListCalculateOnConstants.DealQty });
            AddList.Add(new SelectListItem { Text = RateListCalculateOnConstants.Weight, Value = RateListCalculateOnConstants.Weight });
            ViewBag.CalculateWeightOnList = new SelectList(AddList, "Value", "Text");
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            RateListHeaderViewModel vm = new RateListHeaderViewModel();
            vm.EffectiveDate = DateTime.Now;
            PrepareViewBag();
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(RateListHeaderViewModel vm)
        {
            RateListHeader pt = Mapper.Map<RateListHeaderViewModel, RateListHeader>(vm);
            if (vm.RateListHeaderId <= 0)
                ViewBag.Mode = "Add";
            else
                ViewBag.Mode = "Edit";

            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required.");

            if (ModelState.IsValid)
            {

                if (vm.RateListHeaderId <= 0)
                {
                    int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                    int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


                    pt.SiteId = SiteId;
                    pt.DivisionId = DivisionId;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _RateListHeaderService.Create(pt);

                    if (!string.IsNullOrEmpty(vm.PersonRateGroup))
                        foreach (string item in vm.PersonRateGroup.Split(',').ToList())
                        {
                            RateListPersonRateGroup PRG = new RateListPersonRateGroup();
                            PRG.RateListHeaderId = pt.RateListHeaderId;
                            PRG.PersonRateGroupId = Int32.Parse(item);
                            PRG.CreatedDate = DateTime.Now;
                            PRG.ModifiedDate = DateTime.Now;
                            PRG.CreatedBy = User.Identity.Name;
                            PRG.ModifiedBy = User.Identity.Name;
                            _RateListPersonRateGroupService.Create(PRG);

                        }

                    if (!string.IsNullOrEmpty(vm.ProductRateGroup))
                        foreach (string item in vm.ProductRateGroup.Split(',').ToList())
                        {
                            RateListProductRateGroup PRRG = new RateListProductRateGroup();
                            PRRG.RateListHeaderId = pt.RateListHeaderId;
                            PRRG.ProductRateGroupId = Int32.Parse(item);
                            PRRG.CreatedDate = DateTime.Now;
                            PRRG.ModifiedDate = DateTime.Now;
                            PRRG.CreatedBy = User.Identity.Name;
                            PRRG.ModifiedBy = User.Identity.Name;
                            _RateListProductRateGroupService.Create(PRRG);

                        }

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader).DocumentTypeId,
                        DocId = pt.RateListHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.RateListName,
                    }));

                    return RedirectToAction("Create").Success("Data saved successfully");
                }

                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    RateListHeader temp = _RateListHeaderService.Find(pt.RateListHeaderId);

                    RateListHeader ExRec = Mapper.Map<RateListHeader>(temp);

                    temp.RateListName = pt.RateListName;
                    temp.CalculateWeightageOn = pt.CalculateWeightageOn;
                    temp.DealUnitId = pt.DealUnitId;
                    temp.Description = pt.Description;
                    temp.EffectiveDate = pt.EffectiveDate;
                    temp.MaxRate = pt.MaxRate;
                    temp.MinRate = pt.MinRate;
                    temp.ProcessId = pt.ProcessId;
                    temp.WeightageGreaterOrEqual = pt.WeightageGreaterOrEqual;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;

                    _RateListHeaderService.Update(temp);

                    var RateListPersoRateGroup = _RateListPersonRateGroupService.GetRateListPersonRateGroupList(temp.RateListHeaderId);

                    foreach (var item in RateListPersoRateGroup)
                        _RateListPersonRateGroupService.Delete(item);

                    var RateListProduRateGroup = _RateListProductRateGroupService.GetRateListProductRateGroupList(temp.RateListHeaderId);

                    foreach (var item in RateListProduRateGroup)
                        _RateListProductRateGroupService.Delete(item);

                    if (!string.IsNullOrEmpty(vm.PersonRateGroup))
                        foreach (string item in vm.PersonRateGroup.Split(',').ToList())
                        {
                            RateListPersonRateGroup PRG = new RateListPersonRateGroup();
                            PRG.RateListHeaderId = pt.RateListHeaderId;
                            PRG.PersonRateGroupId = Int32.Parse(item);
                            PRG.CreatedDate = DateTime.Now;
                            PRG.ModifiedDate = DateTime.Now;
                            PRG.CreatedBy = User.Identity.Name;
                            PRG.ModifiedBy = User.Identity.Name;
                            _RateListPersonRateGroupService.Create(PRG);

                        }

                    if (!string.IsNullOrEmpty(vm.ProductRateGroup))
                        foreach (string item in vm.ProductRateGroup.Split(',').ToList())
                        {
                            RateListProductRateGroup PRRG = new RateListProductRateGroup();
                            PRRG.RateListHeaderId = pt.RateListHeaderId;
                            PRRG.ProductRateGroupId = Int32.Parse(item);
                            PRRG.CreatedDate = DateTime.Now;
                            PRRG.ModifiedDate = DateTime.Now;
                            PRRG.CreatedBy = User.Identity.Name;
                            PRRG.ModifiedBy = User.Identity.Name;
                            _RateListProductRateGroupService.Create(PRRG);

                        }


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader).DocumentTypeId,
                        DocId = temp.RateListHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                        DocNo = temp.RateListName,
                    }));

                    return RedirectToAction("Index").Success("Data saved successfully");

                }
            }
            PrepareViewBag();
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            RateListHeaderViewModel pt = _RateListHeaderService.GetRateListHeaderViewModel(id);

            if (pt.CloseDate.HasValue)
                ViewBag.CloseDate = true;

            if (pt == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            ViewBag.Mode = "Edit";
            return View("Create", pt);
        }

        [Authorize]
        public ActionResult Detail(int id, string transactionType)
        {
            RateListHeaderViewModel pt = _RateListHeaderService.GetRateListHeaderViewModel(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            ViewBag.transactionType = transactionType;
            return View("Create", pt);
        }

        public ActionResult Submit(int id, string Redirect)
        {
            return RedirectToAction("Detail", new { id = id, returl = Redirect, transactionType = "submit" });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Submit")]
        public ActionResult Submitted(int Id, string returl, string UserRemark)
        {

            if (ModelState.IsValid)
            {

                RateListHeader Header = _RateListHeaderService.Find(Id);
                int ActivityType;

                var Cartesian = (from p in db.RateListHeader
                                 join t in db.RateListPersonRateGroup on p.RateListHeaderId equals t.RateListHeaderId into table
                                 from tab in table.DefaultIfEmpty()
                                 join t1 in db.RateListProductRateGroup on p.RateListHeaderId equals t1.RateListHeaderId into table2
                                 from tab2 in table2.DefaultIfEmpty()
                                 where p.RateListHeaderId == Id
                                 select new
                                 {
                                     RateListHId = p.RateListHeaderId,
                                     ProdRateGroupId = (int?)tab2.ProductRateGroupId,
                                     PerRateGroupId = (int?)tab.PersonRateGroupId,
                                 }).ToList();


                var ExistingRateList = (from p in db.RateListLine
                                        where p.RateListHeaderId == Id
                                        select p).ToList();

                var PedingToUpdate = (from p in Cartesian
                                      join t in ExistingRateList on new { x = p.RateListHId, y = p.ProdRateGroupId, z = p.PerRateGroupId } equals new { x = t.RateListHeaderId, y = t.ProductRateGroupId, z = t.PersonRateGroupId }
                                      into g
                                      from tab in g.DefaultIfEmpty()
                                      where tab == null
                                      select p).ToList();

                var PendingToDelete = (from p in ExistingRateList
                                       join t in Cartesian on new { x = p.RateListHeaderId, y = p.ProductRateGroupId, z = p.PersonRateGroupId } equals new { x = t.RateListHId, y = t.ProdRateGroupId, z = t.PerRateGroupId }
                                       into g
                                       from tab in g.DefaultIfEmpty()
                                       where tab == null && p.ProductId == null
                                       select p
                                             ).ToList();

                foreach (var item in PedingToUpdate)
                {
                    RateListLine Line = new RateListLine();
                    Line.RateListHeaderId = item.RateListHId;
                    Line.PersonRateGroupId = item.PerRateGroupId;
                    Line.ProductRateGroupId = item.ProdRateGroupId;
                    Line.CreatedBy = User.Identity.Name;
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ModifiedDate = DateTime.Now;
                    Line.ObjectState = Model.ObjectState.Added;
                    db.RateListLine.Add(Line);
                }

                foreach (var item in PendingToDelete)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.RateListLine.Remove(item);
                }

                Header.Status = (int)StatusConstants.Submitted;
                ActivityType = (int)ActivityTypeContants.Submitted;

                //_StockHeaderService.Update(StokHeader);
                Header.ObjectState = Model.ObjectState.Modified;
                db.RateListHeader.Add(Header);

                try
                {
                    db.SaveChanges();
                    db.Dispose();
                }
                catch (Exception Ex)
                {
                    string message = _exception.HandleException(Ex);
                    ModelState.AddModelError("", message);
                    return RedirectToAction("Index").Danger("Error in creating RateList.");
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader).DocumentTypeId,
                    DocId = Header.RateListHeaderId,
                    ActivityType = (int)ActivityTypeContants.Submitted,
                    DocNo = Header.RateListName,
                    UserRemark = UserRemark,
                }));

                return RedirectToAction("RateListWizard", new { id = Id });
            }

            return View();
        }

        public ActionResult RateListWizard(int id)
        {
            RateListHeader Header = _RateListHeaderService.Find(id);

            if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Modified)
            {

                db = new ApplicationDbContext();
                var RateListForWizard = (from p in db.RateListHeader
                                         where p.RateListHeaderId == id
                                         select new RateListHeaderViewModel
                                         {

                                             EffectiveDate = p.EffectiveDate,
                                             RateListName = p.RateListName,
                                             CalculateWeightageOn = p.CalculateWeightageOn,
                                             DealUnitName = p.DealUnit.UnitName,
                                             RateListHeaderId = p.RateListHeaderId,
                                         }).FirstOrDefault();

                return View(RateListForWizard);
            }
            else
            {
                return RedirectToAction("Index").Danger("Please Submit the record");
            }
        }

        public JsonResult GetRateListWizardData(int draw, int start, int length, int id)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            string SortColName = "";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

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

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, ref TOTAL_ROWS, start, length, search, sortColumn, sortDirection, id);
            dataTableData.recordsTotal = TOTAL_ROWS;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }


        private static int TOTAL_ROWS = 0;
        //private static readonly List<DataItem> _data = CreateData();    
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<RateListLineViewModel> data { get; set; }
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
        private List<RateListLineViewModel> FilterData(ref int recordFiltered, ref int recordTotal, int start, int length, string search, int sortColumn, string sortDirection, int id)
        {

            IQueryable<RateListLineViewModel> _data = from p in db.RateListLine
                                                      join t in db.RateListHeader on p.RateListHeaderId equals t.RateListHeaderId
                                                      where p.RateListHeaderId == id
                                                      select new RateListLineViewModel
                                                      {
                                                          ProcessName = t.Process.ProcessName,
                                                          PersonRateGroupName = p.PersonRateGroup.PersonRateGroupName,
                                                          ProductRateGroupName = p.ProductRateGroup.ProductRateGroupName,
                                                          WeightageGreaterOrEqual = t.WeightageGreaterOrEqual,
                                                          Rate = p.Rate,
                                                          Incentive = p.Incentive,
                                                          Discount = p.Discount,
                                                          RateListLineId = p.RateListLineId,
                                                          ProductName = p.Product.ProductName,
                                                      };

            recordTotal = _data.Count();

            List<RateListLineViewModel> list = new List<RateListLineViewModel>();
            if (string.IsNullOrEmpty(search))
            {

            }
            else
            {
                // simulate search
                _data = _data.Where(m => m.ProcessName.ToLower().Contains(search.ToLower())
                    || m.PersonRateGroupName.ToLower().Contains(search.ToLower())
                    || m.ProductName.ToLower().Contains(search.ToLower())
                    || m.ProductRateGroupName.ToLower().Contains(search.ToLower()));
            }

            // simulate sort
            //if (sortColumn == 0)
            //{// sort Name
            _data = _data.OrderBy(m => m.ProcessName).ThenBy(m => m.PersonRateGroupName).ThenBy(m => m.ProductRateGroupName);
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

            recordFiltered = _data.Count();

            // get just one page of data
            return _data.Select(m => new RateListLineViewModel
            {
                ProcessName = m.ProcessName,
                PersonRateGroupName = m.PersonRateGroupName,
                ProductRateGroupName = m.ProductRateGroupName,
                WeightageGreaterOrEqual = m.WeightageGreaterOrEqual,
                Rate = m.Rate,
                Incentive = m.Incentive,
                Discount = m.Discount,
                RateListLineId = m.RateListLineId,
                ProductName = m.ProductName,
            })
            .Skip(start).Take((start == 0) ? 108 : length).ToList();



        }





        public ActionResult UpdateRateList(int LineId, string Type, decimal Value)
        {

            bool Flag = new RateListLineService(_unitOfWork).UpdateRateList(LineId, Type, Value);

            return Json(new { Success = Flag });
        }




        public ActionResult Approve(int id, string Redirect)
        {
            return RedirectToAction("Detail", new { id = id, returl = Redirect, transactionType = "approve" });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Approve")]
        public ActionResult Approved(int Id, string returl)
        {

            if (ModelState.IsValid)
            {
                RateListHeader pd = _RateListHeaderService.Find(Id);



                pd.Status = (int)StatusConstants.Approved;
                pd.ObjectState = Model.ObjectState.Modified;
                db.RateListHeader.Add(pd);

                db.SaveChanges();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader).DocumentTypeId,
                    DocId = pd.RateListHeaderId,
                    ActivityType = (int)ActivityTypeContants.Approved,
                    DocNo = pd.RateListName,
                }));

                return RedirectToAction("Index");
            }

            return View();
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RateListHeader RateListHeader = _RateListHeaderService.Find(id);

            if (RateListHeader == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (ModelState.IsValid)
            {
                var temp = _RateListHeaderService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<RateListHeader>(temp),
                });

                var PersonRateGroupList = _RateListPersonRateGroupService.GetRateListPersonRateGroupList(vm.id);

                foreach (var item in PersonRateGroupList)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<RateListPersonRateGroup>(item),
                    });
                    _RateListPersonRateGroupService.Delete(item.RateListPersonRateGroupId);
                }
                var ProductRateGroupList = _RateListProductRateGroupService.GetRateListProductRateGroupList(vm.id);
                foreach (var item in ProductRateGroupList)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<RateListProductRateGroup>(item),
                    });
                    _RateListProductRateGroupService.Delete(item.RateListProductRateGroupId);
                }

                _RateListHeaderService.Delete(vm.id);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                    DocNo = temp.RateListName,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _RateListHeaderService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _RateListHeaderService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.RateListHeader);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
