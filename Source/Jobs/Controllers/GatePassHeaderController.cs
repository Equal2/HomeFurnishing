using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Xml.Linq;
using CustomEventArgs;
using DocumentEvents;
using GatePassDocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;
namespace Jobs.Controllers
{

    [Authorize]
    public class GatePassHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();
        
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IGatePassHeaderService _GatePassHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public GatePassHeaderController(IGatePassHeaderService GatePassHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _GatePassHeaderService = GatePassHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!GatePassEvents.Initialized)
            {
                GatePassEvents Obj = new GatePassEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        // GET: /GatePassHeader/


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

        public ActionResult Index(int id, string IndexType)//DocumentTypeId 
        {
            //var IpAddr = GetIPAddress();

            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            IQueryable<GatePassHeaderViewModel> p =_GatePassHeaderService.GetGatePassHeaderList(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            ViewBag.id = id;
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<GatePassHeaderViewModel> p = _GatePassHeaderService.GetGatePassHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<GatePassHeaderViewModel> p = _GatePassHeaderService.GetGatePassHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }

        private void PrepareViewBag(int id)
        {
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            int Cid = DocType.DocumentCategoryId;
            ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(Cid).ToList();
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.id = id;
            ViewBag.UnitConvForList = (from p in context.UnitConversonFor
                                       select p).ToList();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        }


        //[HttpGet]
        //public ActionResult BarcodePrint(int id)
        //{

        //    string GenDocId = "";

        //    GatePassHeader header = _GatePassHeaderService.Find(id);
        //    GenDocId = header.DocTypeId.ToString() + '-' + header.DocNo;
        //    //return RedirectToAction("PrintBarCode", "Report_BarcodePrint", new { GenHeaderId = id });
        //    return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + GenDocId + "&queryString=" + GenDocId);


        //}


        //public JsonResult GetJobWorkerHelpList(int Processid, string term)//Order Header ID
        //{

        //    return Json(_GatePassHeaderService.GetJobWorkerHelpList(Processid, term), JsonRequestBehavior.AllowGet);
        //}


        // GET: /GatePassHeader/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
            DocDate.SqlDbType = SqlDbType.DateTime;
            //SqlParameter Godown = new SqlParameter("@GodownId", GoDownId);
            //SqlParameter DocType = new SqlParameter("@DocTypeId", id);
            GatePassHeaderViewModel p = new GatePassHeaderViewModel();    
            p.DocDate = DateTime.Now;        
            p.CreatedDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];            
            p.DocTypeId = id;
            PrepareViewBag(id);

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
            {
                p.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
            }

            //p.DocNo = context.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
            ViewBag.Mode = "Add";
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(GatePassHeaderViewModel svm)
        {
            bool BeforeSave = true;
            bool CostCenterGenerated = false;

            GatePassHeader s = Mapper.Map<GatePassHeaderViewModel, GatePassHeader>(svm);
            #region BeforeSaveEvents
            try
            {

                if (svm.GatePassHeaderId <= 0)
                    BeforeSave = GatePassDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(svm.GatePassHeaderId, EventModeConstants.Add), ref context);
                else
                    BeforeSave = GatePassDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(svm.GatePassHeaderId, EventModeConstants.Edit), ref context);

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

                if (svm.GatePassHeaderId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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
                //CreateLogic
                #region CreateRecord
                if (svm.GatePassHeaderId <= 0)
                {
                    // s.GodownId= (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
                    
                    SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                    DocDate.SqlDbType = SqlDbType.DateTime;
                    SqlParameter Godown = new SqlParameter("@GodownId", svm.GodownId);
                    SqlParameter DocType = new SqlParameter("@DocTypeId",svm.DocTypeId);                    
                    s.DocNo= context.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;   
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Status = (int)StatusConstants.Drafted;
                    s.ObjectState = Model.ObjectState.Added;
                    context.GatePassHeader.Add(s);
                    try
                    {
                        GatePassDocEvents.onHeaderSaveEvent(this, new JobEventArgs(s.GatePassHeaderId, EventModeConstants.Add), ref context);
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
                        //_unitOfWork.Save();
                        context.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                   

                    try
                    {
                        GatePassDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(s.GatePassHeaderId, EventModeConstants.Add), ref context);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = s.DocTypeId,
                        DocId = s.GatePassHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = s.DocNo,
                        DocDate = s.DocDate,
                        DocStatus = s.Status,
                    }));

                    //Update DocId in COstCenter
                 

                    return RedirectToAction("Modify", "GatePassHeader", new { Id = s.GatePassHeaderId }).Success("Data saved successfully");

                }
                #endregion


                //EditLogic
                #region EditRecord

                else
                {
                    bool GodownChanged = false;
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    GatePassHeader temp = context.GatePassHeader.Find(s.GatePassHeaderId);

                    GodownChanged = (temp.GodownId == s.GodownId) ? false : true;

                    GatePassHeader ExRec = Mapper.Map<GatePassHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocDate = s.DocDate;
                    temp.PersonId = s.PersonId;
                    temp.GodownId =s.GodownId;
                    temp.DocNo = s.DocNo;
                    temp.Remark = s.Remark;                 
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    context.GatePassHeader.Add(temp);
                    //_GatePassHeaderService.Update(temp);
                    


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        GatePassDocEvents.onHeaderSaveEvent(this, new JobEventArgs(s.GatePassHeaderId, EventModeConstants.Edit), ref context);
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

                        context.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);

                        PrepareViewBag(svm.DocTypeId);
                        TempData["CSEXC"] += message;
                        ViewBag.id = svm.DocTypeId;
                        ViewBag.Mode = "Edit";
                        return View("Create", svm);
                    }

                    try
                    {
                        GatePassDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(s.GatePassHeaderId, EventModeConstants.Edit), ref context);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.GatePassHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = svm.DocTypeId }).Success("Data saved successfully");

                }
                #endregion

            }
            PrepareViewBag(svm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }


        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            GatePassHeader header = _GatePassHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            GatePassHeader header = _GatePassHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            GatePassHeader header = _GatePassHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            GatePassHeader header = _GatePassHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            GatePassHeader header = _GatePassHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            GatePassHeader header = _GatePassHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, int? DocLineId, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", DocLineId = DocLineId, IndexType = IndexType });
        }



        // GET: /GatePassHeader/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            GatePassHeaderViewModel s = _GatePassHeaderService.GetGatePassHeader(id);
            #region DocTypeTimeLineValidation
            //try
            //{

            //    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    TimePlanValidation = false;
            //}

            //if (!TimePlanValidation)
            //    TempData["CSEXC"] += ExceptionMsg;
            #endregion

            //if ((!TimePlanValidation && !Continue))
            //{
            //    return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            //}

            //Job Order Settings


            // s.GatePassSettings = Mapper.Map<GatePassSettings, GatePassSettingsViewModel>(settings);

            ////Perks
           // s.PerkViewModel = new PerkService(_unitOfWork).GetPerkListForDocumentTypeForEdit(id).ToList();

            //if (s.PerkViewModel.Count == 0)
            //{
            //    List<PerkViewModel> Perks = new List<PerkViewModel>();
            //    if (s.GatePassSettings.Perks != null)
            //        foreach (var item in s.GatePassSettings.Perks.Split(',').ToList())
            //        {
            //            PerkViewModel temp = Mapper.Map<Perk, PerkViewModel>(new PerkService(_unitOfWork).Find(Convert.ToInt32(item)));

            //            var DocTypePerk = (from p2 in context.PerkDocumentType
            //                               where p2.DocTypeId == s.DocTypeId && p2.PerkId == temp.PerkId && p2.SiteId == s.SiteId && p2.DivisionId == s.DivisionId
            //                               select p2).FirstOrDefault();
            //            if (DocTypePerk != null)
            //            {
            //                temp.Base = DocTypePerk.Base;
            //                temp.Worth = DocTypePerk.Worth;
            //                temp.CostConversionMultiplier = DocTypePerk.CostConversionMultiplier;
            //                temp.IsEditableRate = DocTypePerk.IsEditableRate;
            //            }
            //            else
            //            {
            //                temp.Base = 0;
            //                temp.Worth = 0;
            //                temp.CostConversionMultiplier = 0;
            //                temp.IsEditableRate = true;
            //            }

            //            Perks.Add(temp);
            //        }
            //    s.PerkViewModel = Perks;
            //}
            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            //ViewBag.transactionType = "detail";

            ViewBag.Mode = "Edit";
            ViewBag.transactionType = "";

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(s.DocTypeId).DocumentTypeName;
            ViewBag.id = s.DocTypeId;

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.GatePassHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

            return View("Create", s);
        }

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GatePassHeader GatePassHeader = _GatePassHeaderService.Find(id);

            if (GatePassHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(GatePassHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);
        }



        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType, int? DocLineId)
        {
            if (DocLineId.HasValue)
                ViewBag.DocLineId = DocLineId;
            //Saving ViewBag Data::

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            GatePassHeaderViewModel s = _GatePassHeaderService.GetGatePassHeader(id);
            //Getting Settings
            

           

            ////Perks
           

            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.GatePassHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));


            return View("Create", s);
        }


        // GET: /PurchaseOrderHeader/Delete/5



        // POST: /PurchaseOrderHeader/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool BeforeSave = true;

            try
            {
                BeforeSave = GatePassDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref context);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            var GatePassHeader = (from p in context.GatePassHeader
                                  where p.GatePassHeaderId == vm.id
                                  select p).FirstOrDefault();

           

        

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                
                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<GatePassHeader>(GatePassHeader),
                });
                
                var GatePassLine = (from p in context.GatePassLine
                                    where p.GatePassHeaderId == vm.id
                                    select p).ToList();

                var GeLineIds = GatePassLine.Select(m => m.GatePassLineId).ToArray();

                try
                {
                    GatePassDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in GatePassLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<GatePassLine>(item),
                    });





                    //var linecharges = new GatePassLineChargeService(_unitOfWork).GetCalculationProductList(item.GatePassLineId);
                    //foreach (var citem in linecharges)
                    //    new GatePassLineChargeService(_unitOfWork).Delete(citem.Id);

                    item.ObjectState = Model.ObjectState.Deleted;
                    context.GatePassLine.Remove(item);


                }
               

                // Now delete the Purhcase Order Header
                //_GatePassHeaderService.Delete(GatePassHeader);

                int ReferenceDocId = GatePassHeader.GatePassHeaderId;
                int ReferenceDocTypeId = GatePassHeader.DocTypeId;

                GatePassHeader.ObjectState = Model.ObjectState.Deleted;
                context.GatePassHeader.Remove(GatePassHeader);


                //ForDeleting Generated CostCenter:::

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);


                //Commit the DB
                try
                {
                    if (EventException)
                        throw new Exception();

                    context.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    GatePassDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = GatePassHeader.DocTypeId,
                    DocId = GatePassHeader.GatePassHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = GatePassHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = GatePassHeader.DocDate,
                    DocStatus = GatePassHeader.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }


        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {

            #region DocTypeTimeLineValidation
            GatePassHeader s = context.GatePassHeader.Find(id);

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
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue, string GenGatePass)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = GatePassDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref context);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            GatePassHeader pd = context.GatePassHeader.Find(Id);


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int Cnt = 0;
                int CountUid = 0;
                //GatePassHeader pd = new GatePassHeaderService(_unitOfWork).Find(Id);              

                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;                  
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    context.GatePassHeader.Add(pd);
                    try
                    {
                        GatePassDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref context);
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

                        context.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }



                  

                    try
                    {
                        GatePassDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref context);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.GatePassHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));



                    string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.Gatepassheaders", "GatePassHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {
                            var PendingtoSubmitCount = _GatePassHeaderService.GetGatePassHeaderListPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Index_PendingToSubmit" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                            else
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                        }
                        else
                            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Submit" + "/" + nextId + "?TransactionType=submitContinue&IndexType=" + IndexType;
                    }

               

                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
        }


    

        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            GatePassHeader pd = context.GatePassHeader.Find(Id);

            if (ModelState.IsValid)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;
                context.GatePassHeader.Add(pd);

                try
                {
                    GatePassDocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                context.SaveChanges();

                try
                {
                    GatePassDocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.GatePassHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));



                //if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                //{
                //    GatePassHeader HEader = _GatePassHeaderService.Find(Id);

                //    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.GatePassHeaders", "GatePassHeaderId", PrevNextConstants.Next);
                //    if (nextId == 0)
                //    {
                //        var PendingtoSubmitCount = _GatePassHeaderService.GetGatePassHeaderListPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                //        if (PendingtoSubmitCount > 0)
                //            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId });
                //        else
                //            return RedirectToAction("Index", new { id = HEader.DocTypeId });

                //    }

                //    ViewBag.PendingToReview = PendingToReviewCount(Id);
                //    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue" });
                //}
                //else
                //    return RedirectToAction("Index", new { id = pd.DocTypeId }).Success(pd.DocNo + " Reviewed Successfully.");

                string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.Gatepassheaders", "GatePassHeaderId", PrevNextConstants.Next);

                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _GatePassHeaderService.GetGatePassHeaderListPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Index_PendingToReview" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                        else
                            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    }
                    else
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "GatePassHeader" + "/" + "Review" + "/" + nextId + "?TransactionType=ReviewContinue&IndexType=" + IndexType;
                }


                return Redirect(ReturnUrl);

            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in Reviewing.");
        }

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            JobOrderSettings SEttings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            if (!Dt.ReportMenuId.HasValue)
                throw new Exception("Report Menu not configured in document types");

            Model.Models.Menu menu = new MenuService(_unitOfWork).Find(Dt.ReportMenuId ?? 0);

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
                //ReportLine Process = new ReportLineService(_unitOfWork).GetReportLineByName("Process", header.ReportHeaderId);
                //if (Process != null)
                //    DefaultValue.Add(Process.ReportLineId, ((int)SEttings.ProcessId).ToString());
            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        public ActionResult Action_Menu(int Id, int MenuId, string ReturnUrl)
        {
            MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu(MenuId);

            if (menuviewmodel != null)
            {
                if (!string.IsNullOrEmpty(menuviewmodel.URL))
                {
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "?Id=" + Id + "&ReturnUrl=" + ReturnUrl);
                }
                else
                {
                    return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id, ReturnUrl = ReturnUrl });
                }
            }
            return null;
        }

        public int PendingToSubmitCount(int id)
        {
            return (_GatePassHeaderService.GetGatePassHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_GatePassHeaderService.GetGatePassHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.Gatepassheaders", "GatePassHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.Gatepassheaders", "GatePassHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
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
                context.Dispose();
            }
            base.Dispose(disposing);
        }




        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = context.GatePassHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.GatePassHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                        {
                            //LogAct(item.ToString());
                            Pdf = drp.DirectDocumentPrint("Web.spRep_GatePassPrint ", User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                        {
                            Pdf = drp.DirectDocumentPrint("Web.spRep_GatePassPrint ", User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else
                        {
                            Pdf = drp.DirectDocumentPrint("Web.spRep_GatePassPrint ", User.Identity.Name, item);
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
        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

      

    
    }
}
