using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using System.Configuration;
using Presentation;
using Components.ExceptionHandlers;
using Services.BasicSetup;
using Models.Customize.ViewModels;
using Models.Customize.Models;
using Services.Customize;
using TimePlanValidator;
using TimePlanValidator.ViewModels;
using TimePlanValidator.Common;
using ProjLib.Constants;
using Models.BasicSetup.ViewModels;
using CookieNotifier;

namespace Customize.Controllers
{

    [Authorize]
    public class JobOrderHeaderController : System.Web.Mvc.Controller
    {
        List<string> UserRoles = new List<string>();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobOrderHeaderService _JobOrderHeaderService;
        IDocumentTypeService _documentTypeService;
        IJobOrderSettingsService _jobOrderSettingsServie;
        IPerkService _perkService;
        IPerkDocumentTypeService _perkDocumentTypeService;
        IExceptionHandler _exception;
        IDocumentValidation _documentValidation;
        ICostCenterService _costCenterService;

        public JobOrderHeaderController(IJobOrderHeaderService PurchaseOrderHeaderService, IExceptionHandler exec, IDocumentTypeService DocumentTypeServ,
            IJobOrderSettingsService JobOrderSettingsServ, IPerkService perkServ, IPerkDocumentTypeService perkDocTypeServ, IDocumentValidation DocValidation,
            ICostCenterService costCenterSer)
        {
            _JobOrderHeaderService = PurchaseOrderHeaderService;
            _exception = exec;
            _documentTypeService = DocumentTypeServ;
            _jobOrderSettingsServie = JobOrderSettingsServ;
            _perkService = perkServ;
            _perkDocumentTypeService = perkDocTypeServ;
            _documentValidation = DocValidation;
            _costCenterService = costCenterSer;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }

        // GET: /JobOrderHeader/


        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = _documentTypeService.FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

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
            IQueryable<JobOrderHeaderViewModel> p = _JobOrderHeaderService.GetJobOrderHeaderList(id, User.Identity.Name);
            ViewBag.Name = _documentTypeService.Find(id).DocumentTypeName;
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            ViewBag.id = id;
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<JobOrderHeaderViewModel> p = _JobOrderHeaderService.GetJobOrderHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<JobOrderHeaderViewModel> p = _JobOrderHeaderService.GetJobOrderHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }

        private void PrepareViewBag(int id)
        {
            var DocType = _documentTypeService.Find(id);
            int Cid = DocType.DocumentCategoryId;
            ViewBag.DocTypeList = _documentTypeService.FindByDocumentCategory(Cid).ToList();
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.id = id;
            ViewBag.UnitConvForList = _JobOrderHeaderService.GetUnitConversionForList().ToList();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(id, DivisionId, SiteId);
            if (settings != null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.IsPostedInStock = settings.isPostedInStock;
            }

        }


        [HttpGet]
        public ActionResult BarcodePrint(int id)
        {

            string GenDocId = "";

            JobOrderHeader header = _JobOrderHeaderService.Find(id);
            GenDocId = header.DocTypeId.ToString() + '-' + header.DocNo;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["CustomizeDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + GenDocId + "&queryString=" + GenDocId);
        }


        public JsonResult GetJobWorkerHelpList(int Processid, string term)//Order Header ID
        {
            return Json(_JobOrderHeaderService.GetJobWorkerHelpList(Processid, term), JsonRequestBehavior.AllowGet);
        }


        // GET: /JobOrderHeader/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            JobOrderHeaderViewModel p = new JobOrderHeaderViewModel();

            p.DocDate = DateTime.Now;
            p.DueDate = DateTime.Now;
            p.CreatedDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(id, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("Admin"))
            {
                return RedirectToAction("Create", "JobOrderSettings", new { id = id }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("Admin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            p.DocTypeId = id;
            List<PerkViewModel> Perks = new List<PerkViewModel>();
            //Perks
            if (p.JobOrderSettings.Perks != null)
            {
                foreach (var item in p.JobOrderSettings.Perks.Split(',').ToList())
                {
                    PerkViewModel temp = Mapper.Map<Perk, PerkViewModel>(_perkService.Find(Convert.ToInt32(item)));
                    var DocTypePerk = _perkDocumentTypeService.GetPerkDocumentTypeForPerk(id, temp.PerkId, p.SiteId, p.DivisionId);
                    if (DocTypePerk != null)
                    {
                        temp.Base = DocTypePerk.Base;
                        temp.Worth = DocTypePerk.Worth;
                        temp.CostConversionMultiplier = DocTypePerk.CostConversionMultiplier;
                        temp.IsEditableRate = DocTypePerk.IsEditableRate;
                    }
                    else
                    {
                        temp.Base = 0;
                        temp.Worth = 0;
                        temp.CostConversionMultiplier = 0;
                        temp.IsEditableRate = true;
                    }
                    Perks.Add(temp);
                }
            }

            if (p.JobOrderSettings.isVisibleCostCenter)
            {
                p.CostCenterName = _JobOrderHeaderService.FGetJobOrderCostCenter(p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            }

            p.PerkViewModel = Perks;
            p.UnitConversionForId = settings.UnitConversionForId;
            p.ProcessId = settings.ProcessId;

            PrepareViewBag(id);

            //TODO
            //p.OrderById = new EmployeeService(_unitOfWork).GetEmloyeeForUser(User.Identity.GetUserId());

            if (settings.isVisibleGodown == true && System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                p.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            if (settings.DueDays.HasValue)
            {
                p.DueDate = _JobOrderHeaderService.AddDueDate(DateTime.Now, settings.DueDays.Value);
            }
            else
                p.DueDate = DateTime.Now;
            p.DocNo = _documentTypeService.FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobOrderHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            ViewBag.Mode = "Add";
            ViewBag.Name = _documentTypeService.Find(id).DocumentTypeName;
            ViewBag.id = id;
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderHeaderViewModel svm)
        {
            bool BeforeSave = true;

            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(svm.DocTypeId, svm.DivisionId, svm.SiteId);

            if (settings != null)
            {
                if (svm.JobOrderSettings.isMandatoryCostCenter == true && (string.IsNullOrEmpty(svm.CostCenterName)))
                {
                    ModelState.AddModelError("CostCenterName", "The CostCenter field is required");
                }
                if (svm.JobOrderSettings.isMandatoryMachine == true && (svm.MachineId <= 0 || svm.MachineId == null))
                {
                    ModelState.AddModelError("MachineId", "The Machine field is required");
                }
                if (svm.JobOrderSettings.isVisibleGodown == true && svm.JobOrderSettings.isMandatoryGodown == true && !svm.GodownId.HasValue)
                {
                    ModelState.AddModelError("GodownId", "The Godown field is required");
                }
                if (settings.MaxDays.HasValue && (svm.DueDate - svm.DocDate).Days > settings.MaxDays.Value)
                {
                    ModelState.AddModelError("DueDate", "DueDate is exceeding MaxDueDays.");
                }
            }

            if (!string.IsNullOrEmpty(svm.CostCenterName))
            {
                string CostCenterValidation = _JobOrderHeaderService.ValidateCostCenter(svm.DocTypeId, svm.JobOrderHeaderId, svm.JobWorkerId, svm.CostCenterName);
                if (!string.IsNullOrEmpty(CostCenterValidation))
                    ModelState.AddModelError("CostCenterName", CostCenterValidation);
            }

            #region DocTypeTimeLineValidation

            try
            {

                if (svm.JobOrderHeaderId <= 0)
                    TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            if (ModelState.IsValid && BeforeSave && (TimePlanValidation || Continue))
            {
                //CreateLogic
                #region CreateRecord
                if (svm.JobOrderHeaderId <= 0)
                {
                    try
                    {
                        _JobOrderHeaderService.Create(svm, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                    return RedirectToAction("Modify", "JobOrderHeader", new { Id = svm.JobOrderHeaderId }).Success("Data saved successfully");

                }
                #endregion


                //EditLogic
                #region EditRecord

                else
                {
                    try
                    {
                        _JobOrderHeaderService.Update(svm, User.Identity.Name);
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
            JobOrderHeader header = _JobOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobOrderHeader header = _JobOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            JobOrderHeader header = _JobOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobOrderHeader header = _JobOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobOrderHeader header = _JobOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            JobOrderHeader header = _JobOrderHeaderService.Find(id);
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



        // GET: /JobOrderHeader/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            JobOrderHeaderViewModel s = _JobOrderHeaderService.GetJobOrderHeader(id);

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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
            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("Admin"))
            {
                return RedirectToAction("Create", "JobOrderSettings", new { id = s.DocTypeId }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("Admin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            s.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            ////Perks
            s.PerkViewModel = _perkService.GetPerkListForDocumentTypeForEdit(id).ToList();

            if (s.PerkViewModel.Count == 0)
            {
                List<PerkViewModel> Perks = new List<PerkViewModel>();
                if (s.JobOrderSettings.Perks != null)
                    foreach (var item in s.JobOrderSettings.Perks.Split(',').ToList())
                    {
                        PerkViewModel temp = Mapper.Map<Perk, PerkViewModel>(_perkService.Find(Convert.ToInt32(item)));

                        var DocTypePerk = _perkDocumentTypeService.GetPerkDocumentTypeForPerk(s.DocTypeId, temp.PerkId, s.SiteId, s.DivisionId);

                        if (DocTypePerk != null)
                        {
                            temp.Base = DocTypePerk.Base;
                            temp.Worth = DocTypePerk.Worth;
                            temp.CostConversionMultiplier = DocTypePerk.CostConversionMultiplier;
                            temp.IsEditableRate = DocTypePerk.IsEditableRate;
                        }
                        else
                        {
                            temp.Base = 0;
                            temp.Worth = 0;
                            temp.CostConversionMultiplier = 0;
                            temp.IsEditableRate = true;
                        }

                        Perks.Add(temp);
                    }
                s.PerkViewModel = Perks;
            }
            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            ViewBag.Mode = "Edit";
            ViewBag.transactionType = "";

            ViewBag.Name = _documentTypeService.Find(s.DocTypeId).DocumentTypeName;
            ViewBag.id = s.DocTypeId;

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                _JobOrderHeaderService.LogDetailInfo(s);

            return View("Create", s);
        }

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobOrderHeader JobOrderHeader = _JobOrderHeaderService.Find(id);

            if (JobOrderHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation

            try
            {
                TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobOrderHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            JobOrderHeaderViewModel s = _JobOrderHeaderService.GetJobOrderHeader(id);
            //Getting Settings
            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            s.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);

            ////Perks
            s.PerkViewModel = _perkService.GetPerkListForDocumentTypeForEdit(id).ToList();

            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                _JobOrderHeaderService.LogDetailInfo(s);

            return View("Create", s);
        }

        // POST: /PurchaseOrderHeader/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            if (ModelState.IsValid)
            {
                //Commit the DB
                try
                {
                    _JobOrderHeaderService.Delete(vm, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }


        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {

            #region DocTypeTimeLineValidation
            JobOrderHeader s = _JobOrderHeaderService.Find(id);

            try
            {
                TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Submit, User.Identity.Name, out ExceptionMsg, out Continue);
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

            JobOrderHeader pd = _JobOrderHeaderService.Find(Id);


            if (ModelState.IsValid)
            {

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    try
                    {
                        _JobOrderHeaderService.Submit(Id, User.Identity.Name, GenGatePass, UserRemark);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }


                    //string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;

                    //if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    //{

                    //    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobOrderHeaders", "JobOrderHeaderId", PrevNextConstants.Next);

                    //    if (nextId == 0)
                    //    {
                    //        var PendingtoSubmitCount = _JobOrderHeaderService.GetJobOrderHeaderListPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                    //        if (PendingtoSubmitCount > 0)
                    //            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index_PendingToSubmit" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    //        else
                    //            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    //    }
                    //    else
                    //        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Submit" + "/" + nextId + "?TransactionType=submitContinue&IndexType=" + IndexType;
                    //}

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
            JobOrderHeader pd = _JobOrderHeaderService.Find(Id);

            if (ModelState.IsValid)
            {

                _JobOrderHeaderService.Review(Id, User.Identity.Name, UserRemark);

                string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                //if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                //{

                //    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobOrderHeaders", "JobOrderHeaderId", PrevNextConstants.Next);

                //    if (nextId == 0)
                //    {
                //        var PendingtoSubmitCount = _JobOrderHeaderService.GetJobOrderHeaderListPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                //        if (PendingtoSubmitCount > 0)
                //            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index_PendingToReview" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                //        else
                //            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                //    }
                //    else
                //        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Review" + "/" + nextId + "?TransactionType=ReviewContinue&IndexType=" + IndexType;
                //}


                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in Reviewing.");
        }

        [HttpGet]
        public ActionResult Report(int id)
        {

            var Dt = _documentTypeService.Find(id);

            var SEttings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            if (!Dt.ReportMenuId.HasValue)
                throw new Exception("Report Menu not configured in document types");

            var menu = _JobOrderHeaderService.GetMenu(Dt.ReportMenuId ?? 0);

            if (menu != null)
            {
                var header = _JobOrderHeaderService.GetReportHeader(menu.MenuName);

                var Line = _JobOrderHeaderService.GetReportLine("DocumentType", header.ReportHeaderId);
                if (Line != null)
                    DefaultValue.Add(Line.ReportLineId, id.ToString());
                var Site = _JobOrderHeaderService.GetReportLine("Site", header.ReportHeaderId);
                if (Site != null)
                    DefaultValue.Add(Site.ReportLineId, ((int)System.Web.HttpContext.Current.Session["SiteId"]).ToString());
                var Division = _JobOrderHeaderService.GetReportLine("Division", header.ReportHeaderId);
                if (Division != null)
                    DefaultValue.Add(Division.ReportLineId, ((int)System.Web.HttpContext.Current.Session["DivisionId"]).ToString());
            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["CustomizeDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }


        public ActionResult Wizard(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobOrderHeaderViewModel vm = new JobOrderHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.WizardMenuId != null)
                {
                    var menuviewmodel = _JobOrderHeaderService.GetMenu((int)settings.WizardMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }


        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobOrderHeaderViewModel vm = new JobOrderHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.ImportMenuId != null)
                {
                    var menuviewmodel = _JobOrderHeaderService.GetMenu((int)settings.ImportMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = id });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public ActionResult Action_Menu(int Id, int MenuId, string ReturnUrl)
        {
            var menuviewmodel = _JobOrderHeaderService.GetMenu(MenuId);

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
            return (_JobOrderHeaderService.GetJobOrderHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_JobOrderHeaderService.GetJobOrderHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = _JobOrderHeaderService.NextPrevId(DocId, DocTypeId, User.Identity.Name, PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = _JobOrderHeaderService.NextPrevId(DocId, DocTypeId, User.Identity.Name, PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                GenerateCookie.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                _JobOrderHeaderService.Dispose();
            }
            base.Dispose(disposing);
        }

        //public void ExecuteCustomiseEvents(string EventName, object[] Parameters)
        //{
        //    if (EventName != null)
        //    {
        //        string[] FunctionPartArr;
        //        FunctionPartArr = EventName.Split(new Char[] { '.' });

        //        string NameSpace = FunctionPartArr[0];
        //        string ClassName = FunctionPartArr[1];
        //        string FunctionName = FunctionPartArr[2];

        //        object obj = (object)Activator.CreateInstance("CustomiseEvents", NameSpace + "." + ClassName).Unwrap();
        //        Type T = obj.GetType();
        //        T.GetMethod(FunctionName).Invoke(obj, Parameters);
        //    }
        //}


        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;

                var Settings = _jobOrderSettingsServie.GetJobOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);
                string JobHeaderIds = "";

                try
                {

                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {
                        TimePlanValidation = true;

                        var pd = _JobOrderHeaderService.Find(item);

                        if (!pd.GatePassHeaderId.HasValue)
                        {
                            #region DocTypeTimeLineValidation
                            try
                            {

                                TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pd), DocumentTimePlanTypeConstants.GatePassCreate, User.Identity.Name, out ExceptionMsg, out Continue);

                            }
                            catch (Exception ex)
                            {
                                string message = _exception.HandleException(ex);
                                TempData["CSEXC"] += message;
                                TimePlanValidation = false;
                            }
                            #endregion

                            if ((TimePlanValidation || Continue))
                            {
                                if (Settings.isPostedInStock.HasValue && Settings.isPostedInStock == true)
                                {
                                    if (!String.IsNullOrEmpty(Settings.SqlProcGatePass) && pd.Status == (int)StatusConstants.Submitted && !pd.GatePassHeaderId.HasValue)
                                    {
                                        _JobOrderHeaderService.GenerateGatePass(item, User.Identity.Name, pd, Settings.SqlProcGatePass);
                                        JobHeaderIds += pd.JobOrderHeaderId + ", ";
                                    }
                                }
                            }
                            else
                                TempData["CSEXC"] += ExceptionMsg;
                        }
                    }
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty((string)TempData["CSEXC"]))
                    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Gate passes generated successfully");
                else
                    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }




        public ActionResult DeleteGatePass(int Id)
        {            
            if (Id > 0)
            {                
                try
                {
                    var pd = _JobOrderHeaderService.Find(Id);

                    #region DocTypeTimeLineValidation
                    try
                    {

                        TimePlanValidation = _documentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pd), DocumentTimePlanTypeConstants.GatePassCancel, User.Identity.Name, out ExceptionMsg, out Continue);

                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        TimePlanValidation = false;
                    }

                    if (!TimePlanValidation && !Continue)
                        throw new Exception(ExceptionMsg);
                    #endregion

                    if(pd.GatePassHeaderId.HasValue)
                    {
                        _JobOrderHeaderService.DeleteGatePass(pd, User.Identity.Name);
                    }
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Gate pass Deleted successfully");

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                try
                {
                    var MergedPdf = _JobOrderHeaderService.GetReport(Ids, DocTypeId, User.Identity.Name);
                    return File(MergedPdf, "application/pdf");
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult DuplicateCheckForCreate(string docno, int doctypeId)
        {
            var Exists = (_JobOrderHeaderService.CheckForDocNoExists(docno,doctypeId));
            return Json(new { returnvalue = Exists });
        }

        public JsonResult DuplicateCheckForEdit(string docno, int doctypeId, int headerid)
        {
            var Exists = (_JobOrderHeaderService.CheckForDocNoExists(docno, headerid,doctypeId));
            return Json(new { returnvalue = Exists });
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

        public JsonResult ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName)
        {
            return Json(_JobOrderHeaderService.ValidateCostCenter(DocTypeId, HeaderId, JobWorkerId, CostCenterName), JsonRequestBehavior.AllowGet);
        }

    }
}
