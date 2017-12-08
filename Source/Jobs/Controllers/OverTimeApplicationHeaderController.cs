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
using System.Configuration;
using Jobs.Helpers;
using System.Xml.Linq;
//using PurchaseOrderDocumentEvents;
using CustomEventArgs;
//using DocumentEvents;
using Reports.Reports;
using Model.ViewModels;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class OverTimeApplicationHeaderController : System.Web.Mvc.Controller
    {               
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IOverTimeApplicationHeaderService _OverTimeApplicationHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public OverTimeApplicationHeaderController(IOverTimeApplicationHeaderService OverTimeApplicationHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _OverTimeApplicationHeaderService = OverTimeApplicationHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            /*  if (!PurchaseOrderEvents.Initialized)
              {
                  PurchaseOrderEvents Obj = new PurchaseOrderEvents();
              }
              */
            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.DepartmentList = new DepartmentService(_unitOfWork).GetDepartmentList().ToList();
           

            // var EmpIds = new List<int>(_OverTimeApplicationHeaderService.GetListEmpId(vm.DepartmentId)).ToList();
            //ComboBoxResult pt = _OverTimeApplicationHeaderService.GetListEmpId(2);
            ViewBag.PersonList = new PersonService(_unitOfWork).GetPersonList().ToList();
        }

        public JsonResult GetEmployeeList(int id)
        {
                    return Json(_OverTimeApplicationHeaderService.GetListEmpName(id).ToList());
        }



        /* public ActionResult ListLine(int Id)
         {
             var AttendanceLine = _OverTimeApplicationHeaderService.GetAttendanceLineView(Id).ToList();

             var List = AttendanceLine.Select((m, i) => new
             {
                 AttendanceCategory = m.AttendanceCategory,
                 OverTimeApplicationHeaderId = m.OverTimeApplicationHeaderId,
                 AttendanceLineId = m.AttendanceLineId,
                 DocTime = Convert.ToDateTime(m.DocTime).ToString("hh:mm tt"),
                 EmployeeId = m.EmployeeId,
                 Name = m.Name,
                 Remark = m.Remark,
                 id = i,
             }).ToList();

             return Json(List, JsonRequestBehavior.AllowGet);
         }*/
        /* public ActionResult LinePost(AttendanceLinesViewModel pt)
         {
             AttendanceLine ah = _OverTimeApplicationHeaderService.FindLine(pt.AttendanceLineId);
             ah.DocTime = pt.DocTime;
             ah.Remark = pt.Remark;
             ah.AttendanceCategory = pt.AttendanceCategory;
             ah.ModifiedBy = User.Identity.Name;
             ah.ModifiedDate = DateTime.Now;
             ah.ObjectState = Model.ObjectState.Modified;
             db.AttendanceLine.Add(ah);

             try
             {
                 db.SaveChanges();
             }
             catch (Exception ex)
             {
                 string message = _exception.HandleException(ex);
                 ModelState.AddModelError("", message);

             }

             return Json(new { success = true });

         }*/
        public ActionResult Index(int id, string IndexType)//DocumentTypeId
       {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            string name = User.Identity.Name;
            var OverTimeApplicationHeader = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderList(id, name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(OverTimeApplicationHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        // GET: /PurchaseOrderHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeID
        {
            OverTimeApplicationHeaderViewModel vm = new OverTimeApplicationHeaderViewModel();
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            //Getting Settings
            //var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, vm.DivisionId, vm.SiteId);

            /*if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseOrderSetting", new { id = id }).Warning("Please create Purchase Order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            vm.TermsAndConditions = settings.TermsAndConditions;
            vm.CalculateDiscountOnRate = settings.CalculateDiscountOnRate;*/
            vm.DocTypeId = id;
            vm.DocNo = _OverTimeApplicationHeaderService.GetMaxDocNo();
            vm.DocDate = DateTime.Now;
          
            //vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".OverTimeApplicationHeaders", vm.DocTypeId, vm.DocDate, DivisionId, SiteId);

            PrepareViewBag(id);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(OverTimeApplicationHeaderViewModel vm)
        {            
            /* #region BeforeSave
              bool BeforeSave = true;
              try
              {
                  if (vm.OverTimeApplicationHeaderId <= 0)
                      BeforeSave = PurchaseOrderDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseOrderHeaderId, EventModeConstants.Add), ref db);
                  else
                      BeforeSave = PurchaseOrderDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseOrderHeaderId, EventModeConstants.Edit), ref db);
              }
              catch (Exception ex)
              {
                  string message = _exception.HandleException(ex);
                  TempData["CSEXC"] += message;
                  EventException = true;
              }
              if (!BeforeSave)
                  TempData["CSEXC"] += "Failed validation before save"; 
              #endregion*/

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.OverTimeApplicationId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            //&& BeforeSave && !EventException && (TimePlanValidation || Continue)
            if (ModelState.IsValid)
            {

                #region CreateRecord
                if (vm.OverTimeApplicationId <= 0)
                {

                    OverTimeApplicationHeader header = new OverTimeApplicationHeader();
                    header = Mapper.Map<OverTimeApplicationHeaderViewModel, OverTimeApplicationHeader>(vm);
                    header.CreatedBy = User.Identity.Name;
                    header.ModifiedBy = User.Identity.Name;
                    header.CreatedDate = DateTime.Now;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Added;
                    db.OverTimeApplicationHeader.Add(header);
                  foreach (string EmpId in vm.PersonId1.ToString().Split(',').ToArray())
                    {
                        OverTimeApplicationLine Line = new OverTimeApplicationLine();
                        Line.OverTimeApplicationHeaderId = header.OverTimeApplicationId;
                        Line.EmployeeId = Convert.ToInt16(EmpId);
                        Line.Status = 0;
                        Line.CreatedBy = User.Identity.Name;
                        Line.ModifiedBy = User.Identity.Name;
                        Line.CreatedDate = DateTime.Now;
                        Line.ModifiedDate = DateTime.Now;
                        Line.ObjectState = Model.ObjectState.Added;
                        db.OverTimeApplicationLine.Add(Line);
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

                    /* try
                     {
                         PurchaseOrderDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(header.PurchaseOrderHeaderId, EventModeConstants.Add), ref db);
                     }
                     catch (Exception ex)
                     {
                         string message = _exception.HandleException(ex);
                         TempData["CSEXC"] += message;
                     }*/

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.OverTimeApplicationId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));

                    // return RedirectToAction("Modify", new { id = header.OverTimeApplicationHeaderId }).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = header.OverTimeApplicationId }).Success("Data Saved Sucessfully");

                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    OverTimeApplicationHeader temp = _OverTimeApplicationHeaderService.Find(vm.OverTimeApplicationId);


                    OverTimeApplicationHeader ExRec = new OverTimeApplicationHeader();
                    ExRec = Mapper.Map<OverTimeApplicationHeader>(temp);


                    temp.DocNo = vm.DocNo;
                    temp.DocDate = vm.DocDate;
                    temp.DepartmentId = vm.DepartmentId;
                    temp.PersonId = vm.PersonId;
                    temp.Remark = vm.Remark;
                    temp.GodownId = vm.GodownId;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ModifiedDate = DateTime.Now;
                   // temp.ShiftId = vm.ShiftId;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_PurchaseOrderHeaderService.Update(temp);
                    db.OverTimeApplicationHeader.Add(temp);

                    
                    #region deleteLine
                    var line = (from p in db.OverTimeApplicationLine
                                where p.OverTimeApplicationHeaderId == vm.OverTimeApplicationId
                                select p).ToList();

                    foreach (var item in line)
                    {

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = Mapper.Map<OverTimeApplicationLine>(item),
                        });
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.OverTimeApplicationLine.Remove(item);
                    }
                    #endregion

                    #region Update
                    foreach (string EmpId in vm.PersonId1.ToString().Split(',').ToArray())
                    {
                        OverTimeApplicationLine Line = new OverTimeApplicationLine();
                        Line.OverTimeApplicationHeaderId = vm.OverTimeApplicationId;
                        Line.EmployeeId = Convert.ToInt16(EmpId);
                        Line.Status = 0;
                        Line.CreatedBy = User.Identity.Name;
                        Line.ModifiedBy = User.Identity.Name;
                        Line.CreatedDate = DateTime.Now;
                        Line.ModifiedDate = DateTime.Now;
                        Line.ObjectState = Model.ObjectState.Added;
                        db.OverTimeApplicationLine.Add(Line);
                    }
                    #endregion Update

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);


                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                      int i=db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        ViewBag.Mode = "Edit";
                        PrepareViewBag(vm.DocTypeId);
                        return View("Create", vm);
                    }

                    /* try
                     {
                         PurchaseOrderDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseOrderHeaderId, EventModeConstants.Edit), ref db);
                     }
                     catch (Exception ex)
                     {
                         string message = _exception.HandleException(ex);
                         TempData["CSEXC"] += message;
                     }*/

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.OverTimeApplicationId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }
                #endregion

            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {

           

            ViewBag.IndexStatus = IndexType;
            OverTimeApplicationHeaderViewModel pt = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeader(id);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pt), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }
            //Job Order Settings
            //var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            /*if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseOrderSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }*/
            // pt.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.OverTimeApplicationId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            OverTimeApplicationHeader header = _OverTimeApplicationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            OverTimeApplicationHeader header = _OverTimeApplicationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            OverTimeApplicationHeader header = _OverTimeApplicationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            OverTimeApplicationHeader header = _OverTimeApplicationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, int? DocLineId)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", DocLineId = DocLineId });
        }

        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType, int? DocLineId)
        {
            if (DocLineId.HasValue)
                ViewBag.DocLineId = DocLineId;



            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            OverTimeApplicationHeaderViewModel pt = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeader(id);

            PrepareViewBag(pt.DocTypeId);

            //Job Order Settings
            //var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            /* if (settings == null)
             {
                 return RedirectToAction("Create", "PurchaseOrderSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Order settings");
             }
             pt.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
             PrepareViewBag(pt.DocTypeId);
             if (pt == null)
             {
                 return HttpNotFound();
             }*/

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.OverTimeApplicationId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);

        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation            

            OverTimeApplicationHeader s = db.OverTimeApplicationHeader.Find(id);

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Submit, User.Identity.Name, out ExceptionMsg, out Continue);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation && !Continue)
            {
                return RedirectToAction("Index", new { id = s.DocTypeId, IndexType = IndexType });
            }
            #endregion
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "submit" : TransactionType });

        }

        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Submit")]
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            bool BeforeSave = true;
            /* try
             {
                 BeforeSave = PurchaseOrderDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
             }
             catch (Exception ex)
             {
                 string message = _exception.HandleException(ex);
                 TempData["CSEXC"] += message;
                 EventException = true;
             }*/

            /* if (!BeforeSave)
                 TempData["CSEXC"] += "Falied validation before submit.";*/

            OverTimeApplicationHeader pd = new OverTimeApplicationHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                int Cnt = 0;
                int CountUid = 0;

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    int ActivityType;

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                 
                  //  pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.OverTimeApplicationHeader.Add(pd);

                    //try
                    //{
                    //    PurchaseOrderDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //    EventException = true;
                    //}

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
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }

                    //try
                    //{
                    //    PurchaseOrderDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.OverTimeApplicationId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    //NotifyUser(Id, ActivityTypeContants.Submitted);

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {
                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseOrderHeaders", "PurchaseOrderHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {
                            var PendingtoSubmitCount = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType });
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
                        }
                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Order " + pd.DocNo + " submitted successfully.");
                    }
                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Order " + pd.DocNo + " submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }
            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
        }



        public ActionResult Review(int id, string IndexType, string TransactionType)
        {

            ViewBag.PendingToReview = PendingToReviewCount(id);
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });

        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {

            /*bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseOrderDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";*/

            OverTimeApplicationHeader pd = new OverTimeApplicationHeaderService(_unitOfWork).Find(Id);


            if (ModelState.IsValid)
            {
                //pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                //pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;
                db.OverTimeApplicationHeader.Add(pd);


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
                    return RedirectToAction("Index", new { id = pd.DocTypeId });
                }

                /* try
                 {
                     PurchaseOrderDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                 }
                 catch (Exception ex)
                 {
                     string message = _exception.HandleException(ex);
                     TempData["CSEXC"] += message;
                 }*/

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.OverTimeApplicationId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                //NotifyUser(Id, ActivityTypeContants.Approved);


                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    OverTimeApplicationHeader HEader = _OverTimeApplicationHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.OverTimeApplicationHeaders", "OverTimeApplicationHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Record Reviewed Successfully.");
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");

            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
        }


        // GET: /ProductMaster/Delete/5        
        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OverTimeApplicationHeader OverTimeApplicationHeader = _OverTimeApplicationHeaderService.Find(id);
            if (OverTimeApplicationHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(OverTimeApplicationHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation && !Continue)
            {
                return PartialView("AjaxError");
            }
            #endregion

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

            bool BeforeSave = true;
            /* try
             {
                 BeforeSave = PurchaseOrderDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
             }
             catch (Exception ex)
             {
                 string message = _exception.HandleException(ex);
                 TempData["CSEXC"] += message;
                 EventException = true;
             }*/

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            var temp = db.OverTimeApplicationHeader.Find(vm.id);
            // && BeforeSave && !EventException
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                /* try
                 {
                     PurchaseOrderDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                 }
                 catch (Exception ex)
                 {
                     string message = _exception.HandleException(ex);
                     TempData["CSEXC"] += message;
                     EventException = true;
                 }*/

                string Exception = "";

                var line = (from p in db.OverTimeApplicationLine
                            where p.OverTimeApplicationHeaderId == vm.id
                            select p).ToList();

                var LineIds = line.Select(m => m.OverTimeApplicationLineId).ToArray();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<OverTimeApplicationHeader>(temp),
                });

                foreach (var item in line)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<OverTimeApplicationLine>(item),
                    });

                    //new PurchaseOrderLineStatusService(_unitOfWork).Delete(item.PurchaseOrderLineId);

                    //var linecharges = new PurchaseOrderLineChargeService(_unitOfWork).GetCalculationProductList(item.PurchaseOrderLineId);
                    //foreach (var citem in linecharges)
                    //    new PurchaseOrderLineChargeService(_unitOfWork).Delete(citem.Id);


                    //new PurchaseOrderLineService(_unitOfWork).Delete(item.PurchaseOrderLineId);

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.OverTimeApplicationLine.Remove(item);

                }

                /*var headercharges = (from p in db.PurchaseOrderHeaderCharges
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();*/



                /*foreach (var item in headercharges)
                {

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderHeaderCharges.Remove(item);
                    //new PurchaseOrderHeaderChargeService(_unitOfWork).Delete(item.Id);
                }*/
                //new PurchaseOrderHeaderStatusService(_unitOfWork).Delete(temp.PurchaseOrderHeaderId);

                /*var PurchaseOrderHeaderStatus = (from p in db.PurchaseOrderHeaderStatus
                                                 where p.PurchaseOrderHeaderId == vm.id
                                                 select p).FirstOrDefault();*/

                /*if (PurchaseOrderHeaderStatus != null)
                {
                    PurchaseOrderHeaderStatus.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderHeaderStatus.Remove(PurchaseOrderHeaderStatus);
                }*/


                temp.ObjectState = Model.ObjectState.Deleted;
                db.OverTimeApplicationHeader.Remove(temp);

                //_PurchaseOrderHeaderService.Delete(vm.id);

                // XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (!string.IsNullOrEmpty(Exception))
                        throw new Exception(Exception);

                    db.SaveChanges();
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                /*  try
                  {
                      PurchaseOrderDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                  }
                  catch (Exception ex)
                  {
                      string message = _exception.HandleException(ex);
                      TempData["CSEXC"] += message;
                  }*/

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.OverTimeApplicationId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = temp.DocNo,
                    DocDate = temp.DocDate,
                    DocStatus = temp.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.OverTimeApplicationHeaders", "OverTimeApplicationId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.OverTimeApplicationHeaders", "OverTimeApplicationId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History(int id)
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");

        }

        [HttpGet]
        public ActionResult BarcodePrint(int id)
        {

            //return RedirectToAction("PrintBarCode", "Report_BarcodePrint", new { GenHeaderId = id });
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + id + "&queryString=" + id);
        }

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            if (!Dt.ReportMenuId.HasValue)
                throw new Exception("Report Menu not configured in document types");

            Menu menu = new MenuService(_unitOfWork).Find(Dt.ReportMenuId ?? 0);

            if (menu != null)
            {
                ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(menu.MenuName);

                ReportLine Line = new ReportLineService(_unitOfWork).GetReportLineByName("DocumentType", header.ReportHeaderId);
                if (Line != null)
                    DefaultValue.Add(Line.ReportLineId, id.ToString());
                ReportLine Site = new ReportLineService(_unitOfWork).GetReportLineByName("Site", header.ReportHeaderId);
                if (Site != null)
                    DefaultValue.Add(Site.ReportLineId, ((int)System.Web.HttpContext.Current.Session["SiteId"]).ToString());
                ReportLine Division = new ReportLineService(_unitOfWork).GetReportLineByName("Division", header.ReportHeaderId);
                if (Division != null)
                    DefaultValue.Add(Division.ReportLineId, ((int)System.Web.HttpContext.Current.Session["DivisionId"]).ToString());

            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        /* public ActionResult GeneratePrints(string Ids, int DocTypeId)
         {

             if (!string.IsNullOrEmpty(Ids))
             {
                 int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                 int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                 var Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

                 try
                 {

                     List<byte[]> PdfStream = new List<byte[]>();
                     foreach (var item in Ids.Split(',').Select(Int32.Parse))
                     {

                         DirectReportPrint drp = new DirectReportPrint();

                         var pd = db.PurchaseOrderHeader.Find(item);

                         LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                         {
                             DocTypeId = pd.DocTypeId,
                             DocId = pd.PurchaseOrderHeaderId,
                             ActivityType = (int)ActivityTypeContants.Print,
                             DocNo = pd.DocNo,
                             DocDate = pd.DocDate,
                             DocStatus = pd.Status,
                         }));

                         byte[] Pdf;

                         if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                         {
                             //LogAct(item.ToString());
                             Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                             PdfStream.Add(Pdf);
                         }
                         else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                         {
                             Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

                             PdfStream.Add(Pdf);
                         }
                         else
                         {
                             Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
                             PdfStream.Add(Pdf);
                         }

                     }

                     PdfMerger pm = new PdfMerger();

                     byte[] Merge = pm.MergeFiles(PdfStream);

                     if (Merge != null)
                         return File(Merge, "application/pdf");

                 }

                 catch (Exception ex)
                 {
                     string message = _exception.HandleException(ex);
                     return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                 }


                 return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

             }
             return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

         }
         [HttpGet]
         public ActionResult Email()
         {
             //To Be Implemented
             return View("~/Views/Shared/UnderImplementation.cshtml");
         }
         */

        /*public ActionResult Action_OnSubmit(int Id, int DocTypeId)//DocId
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.OnSubmitMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.OnSubmitMenuId);

                    if (menuviewmodel != null)
                    {
                        if (!string.IsNullOrEmpty(menuviewmodel.URL))
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id);
                        }
                        else
                        {
                            return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { id = DocTypeId });
        }
        */
        /*
        public ActionResult Action_OnApprove(int Id, int DocTypeId)//DocId
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.OnApproveMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.OnApproveMenuId);

                    if (menuviewmodel != null)
                    {
                        if (!string.IsNullOrEmpty(menuviewmodel.URL))
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id);
                        }
                        else
                        {
                            return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { id = DocTypeId });
        }
        */
        /* private void NotifyUser(int Id, ActivityTypeContants ActivityType)
         {
             OverTimeApplicationHeader Header = new OverTimeApplicationHeaderService(_unitOfWork).Find(Id);
             PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

             DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
             DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
             DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);

             new NotifyUserController(_unitOfWork).SendEmailMessage(Id, ActivityType, DocEmailContentSettings, PurchaseOrderSettings.SqlProcDocumentPrint);
             new NotifyUserController(_unitOfWork).SendNotificationMessage(Id, ActivityType, DocNotificationContentSettings, User.Identity.Name);
             new NotifyUserController(_unitOfWork).SendSmsMessage(Id, ActivityType, DocSmsContentSettings);

         }*/

        //private void SendEmailMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocEmailContentSettings != null)
        //    {
        //        if (DocEmailContentSettings.ProcEmailContent != null && DocEmailContentSettings.ProcEmailContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<EmailContentViewModel> MailContent = db.Database.SqlQuery<EmailContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocEmailContentSettings.ProcEmailContent + " @Id", SqlParameterId);

        //            foreach (EmailContentViewModel item in MailContent)
        //            {
        //                if (DocEmailContentSettings.AttachmentTypes != null && DocEmailContentSettings.AttachmentTypes != "")
        //                {
        //                    string[] AttachmentTypeArr = DocEmailContentSettings.AttachmentTypes.Split(',');

        //                    for (int i = 0; i <= AttachmentTypeArr.Length - 1; i++)
        //                    {
        //                        if (item.FileNameList != "" && item.FileNameList != null) { item.FileNameList = item.FileNameList + ","; }
        //                        if (AttachmentTypeArr[i].ToUpper() == "PDF")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else if (AttachmentTypeArr[i].ToUpper() == "EXCEL")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.Excel, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else if (AttachmentTypeArr[i].ToUpper() == "WORD")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.Word, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                    }
        //                    item.EmailBody = item.EmailBody.Replace("DomainName", (string)System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"]);
        //                }

        //                SendEmail.SendEmailMsg(item);
        //            }
        //        }
        //    }
        //}

        //private void SendNotificationMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocNotificationContentSettings != null)
        //    {
        //        if (DocNotificationContentSettings.ProcNotificationContent != null && DocNotificationContentSettings.ProcNotificationContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<NotificationContentViewModel> NotificationContent = db.Database.SqlQuery<NotificationContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocNotificationContentSettings.ProcNotificationContent + " @Id", SqlParameterId);

        //            foreach (NotificationContentViewModel item in NotificationContent)
        //            {
        //                Notification Note = new Notification();
        //                if (ActivityType == ActivityTypeContants.Submitted)
        //                {
        //                    Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderSubmitted;
        //                }
        //                else
        //                {
        //                    Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderApproved;
        //                }
        //                Note.NotificationText = item.NotificationText;
        //                Note.NotificationUrl = item.NotificationUrl;
        //                Note.UrlKey = item.UrlKey;
        //                Note.ExpiryDate = item.ExpiryDate;
        //                Note.IsActive = true;
        //                Note.CreatedBy = User.Identity.Name;
        //                Note.ModifiedBy = User.Identity.Name;
        //                Note.CreatedDate = DateTime.Now;
        //                Note.ModifiedDate = DateTime.Now;
        //                new NotificationService(_unitOfWork).Create(Note);

        //                string[] UserNameArr = item.UserNameList.Split(',');

        //                foreach (string UserName in UserNameArr)
        //                {
        //                    NotificationUser NoteUser = new NotificationUser();
        //                    NoteUser.NotificationId = Note.NotificationId;
        //                    NoteUser.UserName = UserName;
        //                    new NotificationUserService(_unitOfWork).Create(NoteUser);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void SendSmsMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocSmsContentSettings != null)
        //    {
        //        if (DocSmsContentSettings.ProcSmsContent != null && DocSmsContentSettings.ProcSmsContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<SmsContentViewModel> SmsContent = db.Database.SqlQuery<SmsContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocSmsContentSettings.ProcSmsContent + " @Id", SqlParameterId);

        //            foreach (SmsContentViewModel item in SmsContent)
        //            {

        //            }
        //        }
        //    }
        //}

        public int PendingToSubmitCount(int id)
        {
            return (_OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_OverTimeApplicationHeaderService.GetOverTimeApplicationHeaderPendingToReview(id, User.Identity.Name)).Count();
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
