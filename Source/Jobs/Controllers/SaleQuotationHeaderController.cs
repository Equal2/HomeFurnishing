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
using SaleQuotationDocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;




namespace Jobs.Controllers
{

    [Authorize]
    public class SaleQuotationHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleQuotationHeaderService _SaleQuotationHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SaleQuotationHeaderController(ISaleQuotationHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleQuotationHeaderService = PurchaseOrderHeaderService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!SaleQuotationEvents.Initialized)
            {
                SaleQuotationEvents Obj = new SaleQuotationEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        // GET: /SaleQuotationHeader/


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
            IQueryable<SaleQuotationHeaderViewModel> p = _SaleQuotationHeaderService.GetSaleQuotationHeaderList(id, User.Identity.Name);
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
            IQueryable<SaleQuotationHeaderViewModel> p = _SaleQuotationHeaderService.GetSaleQuotationHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<SaleQuotationHeaderViewModel> p = _SaleQuotationHeaderService.GetSaleQuotationHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }

        private void PrepareViewBag(int id)
        {
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            DocumentTypeSettingsViewModel DTS = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(id);
           
            int Cid = DocType.DocumentCategoryId;
            ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(Cid).ToList();
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.PartyCaption = DTS.PartyCaption;
            ViewBag.id = id;
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
              ViewBag.AdminSetting =UserRoles.Contains("Admin").ToString();
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(id, DivisionId, SiteId);
            if (settings != null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.isVisibleCostCenter = settings.isVisibleCostCenter;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }



        }


        [HttpGet]
        public ActionResult BarcodePrint(int id)
        {

            string GenDocId = "";

            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
            GenDocId = header.DocTypeId.ToString() + '-' + header.DocNo;
            //return RedirectToAction("PrintBarCode", "Report_BarcodePrint", new { GenHeaderId = id });
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + GenDocId + "&queryString=" + GenDocId);


        }




        // GET: /SaleQuotationHeader/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            SaleQuotationHeaderViewModel p = new SaleQuotationHeaderViewModel();
            p.DocDate = DateTime.Now;
            p.DueDate = DateTime.Now;
            p.CreatedDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            List<DocumentTypeHeaderAttributeViewModel> tem = new DocumentTypeService(_unitOfWork).GetDocumentTypeHeaderAttribute(id).ToList();
            p.DocumentTypeHeaderAttributes = tem;

            //Getting Settings
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(id, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleQuotationSettings", new { id = id }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }


            p.DocTypeId = id;
            p.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(p.DocTypeId);

            p.BuyerDocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Prospect).DocumentTypeId;
            

            if (p.SaleQuotationSettings.isVisibleCostCenter)
            {
                p.CostCenterName = new SaleQuotationHeaderService(_unitOfWork).FGetSaleQuotationCostCenter(p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            }

            p.UnitConversionForId = settings.UnitConversionForId;
            p.ProcessId = settings.ProcessId;
            p.TermsAndConditions = settings.TermsAndConditions;
            p.ShipMethodId = settings.ShipMethodId;
            p.DeliveryTermsId = settings.DeliveryTermsId;
            p.CurrencyId = settings.CurrencyId;

            PrepareViewBag(id);

            p.DueDate = DateTime.Now;
            p.ExpiryDate = DateTime.Now;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleQuotationHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            ViewBag.Mode = "Add";
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleQuotationHeaderViewModel svm)
        {
            bool BeforeSave = true;
            bool CostCenterGenerated = false;

            SaleQuotationHeader s = Mapper.Map<SaleQuotationHeaderViewModel, SaleQuotationHeader>(svm);
            SaleQuotationHeaderDetail sd = Mapper.Map<SaleQuotationHeaderViewModel, SaleQuotationHeaderDetail>(svm);

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(svm.DocTypeId, svm.DivisionId, svm.SiteId);

            if (settings != null)
            {
                if (svm.SaleQuotationSettings.isVisiblePaymentTerms == true)
                {
                    if ((svm.PayTermAdvancePer ?? 0) + (svm.PayTermOnDeliveryPer ?? 0) + (svm.PayTermOnDueDatePer ?? 0) > 100)
                    {
                        ModelState.AddModelError("PayTermAdvancePer", "Total of Advance %, Delivery % and Due Date % should be equal to 100.");
                    }
                    if ((svm.PayTermBankPer ?? 0) + (svm.PayTermCashPer ?? 0) > 100)
                    {
                        ModelState.AddModelError("PayTermAdvancePer", "Total of Bank %, and Cash % should be equal to 100.");
                    }
                }
            }

            if (!string.IsNullOrEmpty(svm.CostCenterName))
            {
                string CostCenterValidation = _SaleQuotationHeaderService.ValidateCostCenter(svm.DocTypeId, svm.SaleQuotationHeaderId, svm.SaleToBuyerId, svm.CostCenterName);
                if (!string.IsNullOrEmpty(CostCenterValidation))
                    ModelState.AddModelError("CostCenterName", CostCenterValidation);
            }


            #region BeforeSaveEvents

            try
            {

                if (svm.SaleQuotationHeaderId <= 0)
                    BeforeSave = SaleQuotationDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(svm.SaleQuotationHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = SaleQuotationDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(svm.SaleQuotationHeaderId, EventModeConstants.Edit), ref db);

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

                if (svm.SaleQuotationHeaderId <= 0)
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
                if (svm.SaleQuotationHeaderId <= 0)
                {

                    if (!string.IsNullOrEmpty(svm.CostCenterName))
                    {

                        var CostCenter = new CostCenterService(_unitOfWork).Find(svm.CostCenterName, svm.DivisionId, svm.SiteId, svm.DocTypeId);
                        if (CostCenter != null)
                        {
                            s.CostCenterId = CostCenter.CostCenterId;
                            if (s.CostCenterId.HasValue)
                            {
                                var costcen = new CostCenterService(_unitOfWork).Find(s.CostCenterId.Value);
                                costcen.ProcessId = svm.ProcessId;
                                costcen.ObjectState = Model.ObjectState.Modified;
                                db.CostCenter.Add(costcen);
                                //new CostCenterService(_unitOfWork).Update(costcen);
                            }
                        }
                        else
                        {
                            CostCenter Cs = new CostCenter();
                            Cs.CostCenterName = svm.CostCenterName;
                            Cs.DivisionId = svm.DivisionId;
                            Cs.SiteId = svm.SiteId;
                            Cs.DocTypeId = svm.DocTypeId;
                            Cs.ProcessId = svm.ProcessId;
                            Cs.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(svm.SaleToBuyerId).LedgerAccountId;
                            Cs.CreatedBy = User.Identity.Name;
                            Cs.ModifiedBy = User.Identity.Name;
                            Cs.CreatedDate = DateTime.Now;
                            Cs.ModifiedDate = DateTime.Now;
                            Cs.IsActive = true;
                            Cs.ReferenceDocNo = svm.DocNo;
                            Cs.ReferenceDocTypeId = svm.DocTypeId;
                            Cs.StartDate = svm.DocDate;
                            Cs.ParentCostCenterId = new ProcessService(_unitOfWork).Find(svm.ProcessId).CostCenterId;
                            Cs.ObjectState = Model.ObjectState.Added;
                            //new CostCenterService(_unitOfWork).Create(Cs);
                            db.CostCenter.Add(Cs);
                            s.CostCenterId = Cs.CostCenterId;

                            new CostCenterStatusService(_unitOfWork).CreateLineStatus(Cs.CostCenterId, ref db, true);
                            CostCenterGenerated = true;

                        }

                    }


                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Status = (int)StatusConstants.Drafted;
                    s.ObjectState = Model.ObjectState.Added;
                    db.SaleQuotationHeader.Add(s);


                    sd.SaleQuotationHeaderId = s.SaleQuotationHeaderId;
                    sd.ObjectState = Model.ObjectState.Added;
                    db.SaleQuotationHeaderDetail.Add(sd);


                    if (svm.DocumentTypeHeaderAttributes != null)
                    {
                        foreach (var pta in svm.DocumentTypeHeaderAttributes)
                        {

                            SaleQuotationHeaderAttributes SaleQuotationHeaderAttribute = (from A in db.SaleQuotationHeaderAttributes
                                                                                          where A.HeaderTableId == s.SaleQuotationHeaderId && A.DocumentTypeHeaderAttributeId == pta.DocumentTypeHeaderAttributeId
                                                                                      select A).FirstOrDefault();

                            if (SaleQuotationHeaderAttribute != null)
                            {
                                SaleQuotationHeaderAttribute.Value = pta.Value;
                                SaleQuotationHeaderAttribute.ObjectState = Model.ObjectState.Modified;
                                //_unitOfWork.Repository<SaleQuotationHeaderAttributes>().Add(SaleQuotationHeaderAttribute);
                                db.SaleQuotationHeaderAttributes.Add(SaleQuotationHeaderAttribute);
                            }
                            else
                            {
                                SaleQuotationHeaderAttributes HeaderAttribute = new SaleQuotationHeaderAttributes()
                                {
                                    Value = pta.Value,
                                    HeaderTableId = s.SaleQuotationHeaderId,
                                    DocumentTypeHeaderAttributeId = pta.DocumentTypeHeaderAttributeId,
                                };
                                HeaderAttribute.ObjectState = Model.ObjectState.Added;
                                db.SaleQuotationHeaderAttributes.Add(HeaderAttribute);
                                _unitOfWork.Repository<SaleQuotationHeaderAttributes>().Add(HeaderAttribute);
                            }
                        }
                    }


                    try
                    {
                        SaleQuotationDocEvents.onHeaderSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, EventModeConstants.Add), ref db);
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
                        db.SaveChanges();
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
                        SaleQuotationDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = s.DocTypeId,
                        DocId = s.SaleQuotationHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = s.DocNo,
                        DocDate = s.DocDate,
                        DocStatus = s.Status,
                    }));

                    //Update DocId in COstCenter
                    if (s.CostCenterId.HasValue && CostCenterGenerated)
                    {
                        var CC = db.CostCenter.Find(s.CostCenterId);
                        CC.ReferenceDocId = s.SaleQuotationHeaderId;
                        CC.ObjectState = Model.ObjectState.Modified;
                        db.CostCenter.Add(CC);

                        db.SaveChanges();
                        //new CostCenterService(_unitOfWork).Update(CC);
                        //_unitOfWork.Save();
                    }

                    return RedirectToAction("Modify", "SaleQuotationHeader", new { Id = s.SaleQuotationHeaderId }).Success("Data saved successfully");

                }
                #endregion


                //EditLogic
                #region EditRecord

                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleQuotationHeader temp = db.SaleQuotationHeader.Find(s.SaleQuotationHeaderId);
                    SaleQuotationHeaderDetail HeaderDetail = db.SaleQuotationHeaderDetail.Find(s.SaleQuotationHeaderId);

                    SaleQuotationHeader ExRec = Mapper.Map<SaleQuotationHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    if (!string.IsNullOrEmpty(svm.CostCenterName))
                    {
                        var CostCenter = new CostCenterService(_unitOfWork).Find(svm.CostCenterName, svm.DivisionId, svm.SiteId, svm.DocTypeId);
                        if (CostCenter != null)
                        {
                            temp.CostCenterId = CostCenter.CostCenterId;
                            if (temp.CostCenterId.HasValue)
                            {
                                //var costcen = new CostCenterService(_unitOfWork).Find(temp.CostCenterId.Value);
                                var costcen = (from p in db.CostCenter
                                               where p.CostCenterId == temp.CostCenterId
                                               select p).FirstOrDefault();
                                costcen.ProcessId = svm.ProcessId;
                                costcen.LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(svm.SaleToBuyerId).LedgerAccountId;

                                costcen.ObjectState = Model.ObjectState.Modified;
                                db.CostCenter.Add(costcen);
                                //new CostCenterService(_unitOfWork).Update(costcen);
                            }
                        }
                        else
                        {

                            var ExistingCostCenter = db.CostCenter.Find(temp.CostCenterId);

                            ExistingCostCenter.CostCenterName = svm.CostCenterName;
                            ExistingCostCenter.ObjectState = Model.ObjectState.Modified;

                            db.CostCenter.Add(ExistingCostCenter);
                        }

                    }




                    temp.DocDate = s.DocDate;
                    temp.DueDate = s.DueDate;
                    temp.UnitConversionForId = s.UnitConversionForId;
                    temp.ProcessId = s.ProcessId;
                    temp.SaleToBuyerId = s.SaleToBuyerId;
                    temp.TermsAndConditions = s.TermsAndConditions;
                    temp.DocNo = s.DocNo;
                    temp.CurrencyId = s.CurrencyId;
                    temp.SalesTaxGroupPersonId = s.SalesTaxGroupPersonId;
                    temp.Remark = s.Remark;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.SaleQuotationHeader.Add(temp);
                    //_SaleQuotationHeaderService.Update(temp);


                    HeaderDetail.DeliveryTermsId = sd.DeliveryTermsId;
                    HeaderDetail.ShipMethodId = sd.ShipMethodId;
                    HeaderDetail.IsDoorDelivery = sd.IsDoorDelivery;
                    HeaderDetail.AgentId = sd.AgentId;
                    HeaderDetail.PayTermAdvancePer = sd.PayTermAdvancePer;
                    HeaderDetail.PayTermOnDeliveryPer = sd.PayTermOnDeliveryPer;
                    HeaderDetail.PayTermOnDueDatePer = sd.PayTermOnDueDatePer;
                    HeaderDetail.PayTermCashPer = sd.PayTermCashPer;
                    HeaderDetail.PayTermBankPer = sd.PayTermBankPer;
                    HeaderDetail.FinancierId = sd.FinancierId;
                    HeaderDetail.SalesExecutiveId = sd.SalesExecutiveId;
                    HeaderDetail.CreditDays = sd.CreditDays;
                    HeaderDetail.ObjectState = Model.ObjectState.Modified;
                    db.SaleQuotationHeaderDetail.Add(HeaderDetail);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });


                    if (svm.DocumentTypeHeaderAttributes != null)
                    {
                        foreach (var pta in svm.DocumentTypeHeaderAttributes)
                        {

                            SaleQuotationHeaderAttributes SaleQuotationHeaderAttribute = (from A in db.SaleQuotationHeaderAttributes
                                                                                          where A.HeaderTableId == temp.SaleQuotationHeaderId && A.DocumentTypeHeaderAttributeId == pta.DocumentTypeHeaderAttributeId
                                                                                      select A).FirstOrDefault();

                            if (SaleQuotationHeaderAttribute != null)
                            {
                                SaleQuotationHeaderAttribute.Value = pta.Value;
                                SaleQuotationHeaderAttribute.ObjectState = Model.ObjectState.Modified;
                                //_unitOfWork.Repository<SaleQuotationHeaderAttributes>().Add(SaleQuotationHeaderAttribute);
                                db.SaleQuotationHeaderAttributes.Add(SaleQuotationHeaderAttribute);
                            }
                            else
                            {
                                SaleQuotationHeaderAttributes HeaderAttribute = new SaleQuotationHeaderAttributes()
                                {
                                    Value = pta.Value,
                                    HeaderTableId = temp.SaleQuotationHeaderId,
                                    DocumentTypeHeaderAttributeId = pta.DocumentTypeHeaderAttributeId,
                                };
                                HeaderAttribute.ObjectState = Model.ObjectState.Added;
                                db.SaleQuotationHeaderAttributes.Add(HeaderAttribute);
                            }
                        }
                    }



                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        SaleQuotationDocEvents.onHeaderSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, EventModeConstants.Edit), ref db);
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

                        PrepareViewBag(svm.DocTypeId);
                        TempData["CSEXC"] += message;
                        ViewBag.id = svm.DocTypeId;
                        ViewBag.Mode = "Edit";
                        return View("Create", svm);
                    }

                    try
                    {
                        SaleQuotationDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(s.SaleQuotationHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleQuotationHeaderId,
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
            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            SaleQuotationHeader header = _SaleQuotationHeaderService.Find(id);
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



        // GET: /SaleQuotationHeader/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            SaleQuotationHeaderViewModel s = _SaleQuotationHeaderService.GetSaleQuotationHeader(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleQuotationSettings", new { id = s.DocTypeId }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            s.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(s.DocTypeId);

            ////Perks

            
            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }


            List<DocumentTypeHeaderAttributeViewModel> tem = _SaleQuotationHeaderService.GetDocumentHeaderAttribute(id).ToList();
            s.DocumentTypeHeaderAttributes = tem;

            s.CalculationFooterChargeCount = new SaleQuotationHeaderChargeService(_unitOfWork).GetCalculationFooterList(id).Count();

            //ViewBag.transactionType = "detail";

            ViewBag.Mode = "Edit";
            ViewBag.transactionType = "";

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(s.DocTypeId).DocumentTypeName;
            ViewBag.id = s.DocTypeId;

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.SaleQuotationHeaderId,
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
            SaleQuotationHeader SaleQuotationHeader = _SaleQuotationHeaderService.Find(id);

            if (SaleQuotationHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, SaleQuotationHeader.DocTypeId, SaleQuotationHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleQuotationHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            SaleQuotationHeaderViewModel s = _SaleQuotationHeaderService.GetSaleQuotationHeader(id);
            //Getting Settings
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            s.SaleQuotationSettings = Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(s.DocTypeId);



            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.SaleQuotationHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));


            return View("Create", s);
        }





        // POST: /PurchaseOrderHeader/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            bool BeforeSave = true;

            try
            {
                BeforeSave = SaleQuotationDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            var SaleQuotationHeader = (from p in db.SaleQuotationHeader
                                  where p.SaleQuotationHeaderId == vm.id
                                  select p).FirstOrDefault();


            var attributes = (from A in db.SaleQuotationHeaderAttributes where A.HeaderTableId == vm.id select A).ToList();

            foreach (var ite2 in attributes)
            {
                ite2.ObjectState = Model.ObjectState.Deleted;
                db.SaleQuotationHeaderAttributes.Remove(ite2);
            }



            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                int? StockHeaderId = 0;


                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)


                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<SaleQuotationHeader>(SaleQuotationHeader),
                });


                //Then find all the Purchase Order Header Line associated with the above ProductType.
                //var SaleQuotationLine = new SaleQuotationLineService(_unitOfWork).GetSaleQuotationLineforDelete(vm.id);
                var SaleQuotationLine = (from p in db.SaleQuotationLine
                                    where p.SaleQuotationHeaderId == vm.id
                                    select p).ToList();

                var JOLineIds = SaleQuotationLine.Select(m => m.SaleQuotationLineId).ToArray();



                var LineChargeRecords = (from p in db.SaleQuotationLineCharge
                                         where JOLineIds.Contains(p.LineTableId)
                                         select p).ToList();

                var HeaderChargeRecords = (from p in db.SaleQuotationHeaderCharge
                                           where p.HeaderTableId == vm.id
                                           select p).ToList();


                try
                {
                    SaleQuotationDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }





                foreach (var item in LineChargeRecords)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.SaleQuotationLineCharge.Remove(item);
                }





                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in SaleQuotationLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<SaleQuotationLine>(item),
                    });



                    item.ObjectState = Model.ObjectState.Deleted;
                    db.SaleQuotationLine.Remove(item);


                }





                foreach (var item in HeaderChargeRecords)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.SaleQuotationHeaderCharge.Remove(item);
                }

                var SaleQuotationHeaderDetail = (from p in db.SaleQuotationHeaderDetail
                                            where p.SaleQuotationHeaderId == vm.id
                                            select p).FirstOrDefault();

                SaleQuotationHeaderDetail.ObjectState = Model.ObjectState.Deleted;
                db.SaleQuotationHeaderDetail.Remove(SaleQuotationHeaderDetail);

                // Now delete the Purhcase Order Header
                //_SaleQuotationHeaderService.Delete(SaleQuotationHeader);

                int ReferenceDocId = SaleQuotationHeader.SaleQuotationHeaderId;
                int ReferenceDocTypeId = SaleQuotationHeader.DocTypeId;


                SaleQuotationHeader.ObjectState = Model.ObjectState.Deleted;
                db.SaleQuotationHeader.Remove(SaleQuotationHeader);


                //ForDeleting Generated CostCenter:::

                var GeneratedCostCenter = (from p in db.CostCenter
                                           where p.ReferenceDocId == ReferenceDocId && p.ReferenceDocTypeId == ReferenceDocTypeId
                                           select p).FirstOrDefault();

                if (GeneratedCostCenter != null)
                {
                    var CostCentrerStatusRecord = (from p in db.CostCenterStatus
                                                   where p.CostCenterId == GeneratedCostCenter.CostCenterId
                                                   select p).FirstOrDefault();

                    if (CostCentrerStatusRecord != null)
                    {
                        CostCentrerStatusRecord.ObjectState = Model.ObjectState.Deleted;
                        db.CostCenterStatus.Remove(CostCentrerStatusRecord);
                    }
                    GeneratedCostCenter.ObjectState = Model.ObjectState.Deleted;
                    db.CostCenter.Remove(GeneratedCostCenter);
                }



                var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(SaleQuotationHeader.DocTypeId, SaleQuotationHeader.DivisionId, SaleQuotationHeader.SiteId);


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);


                //Commit the DB
                try
                {
                    if (EventException)
                        throw new Exception();

                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    SaleQuotationDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = SaleQuotationHeader.DocTypeId,
                    DocId = SaleQuotationHeader.SaleQuotationHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = SaleQuotationHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = SaleQuotationHeader.DocDate,
                    DocStatus = SaleQuotationHeader.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }


        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            SaleQuotationHeader s = db.SaleQuotationHeader.Find(id);
            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            
            try
            {
                TimePlanValidation = Submitvalidation(id, out ExceptionMsg);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }


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
                BeforeSave = SaleQuotationDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            SaleQuotationHeader pd = db.SaleQuotationHeader.Find(Id);


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int Cnt = 0;
                int CountUid = 0;
                //SaleQuotationHeader pd = new SaleQuotationHeaderService(_unitOfWork).Find(Id);              

                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    SaleQuotationSettings Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);





                    //_SaleQuotationHeaderService.Update(pd);
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.SaleQuotationHeader.Add(pd);



                    try
                    {
                        SaleQuotationDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }




                    try
                    {
                        SaleQuotationDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.SaleQuotationHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));



                    return Redirect(System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "SaleQuotationHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType);
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
            SaleQuotationHeader pd = db.SaleQuotationHeader.Find(Id);

            if (ModelState.IsValid)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;
                db.SaleQuotationHeader.Add(pd);

                try
                {
                    SaleQuotationDocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                db.SaveChanges();

                try
                {
                    SaleQuotationDocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.SaleQuotationHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));



                //if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                //{
                //    SaleQuotationHeader HEader = _SaleQuotationHeaderService.Find(Id);

                //    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.SaleQuotationHeaders", "SaleQuotationHeaderId", PrevNextConstants.Next);
                //    if (nextId == 0)
                //    {
                //        var PendingtoSubmitCount = _SaleQuotationHeaderService.GetSaleQuotationHeaderListPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
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

                string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "SaleQuotationHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.SaleQuotationHeaders", "SaleQuotationHeaderId", PrevNextConstants.Next);

                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _SaleQuotationHeaderService.GetSaleQuotationHeaderListPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "SaleQuotationHeader" + "/" + "Index_PendingToReview" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                        else
                            ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "SaleQuotationHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    }
                    else
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "SaleQuotationHeader" + "/" + "Review" + "/" + nextId + "?TransactionType=ReviewContinue&IndexType=" + IndexType;
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

            SaleQuotationSettings SEttings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

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


        public ActionResult Wizard(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            SaleQuotationHeaderViewModel vm = new SaleQuotationHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.WizardMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.WizardMenuId);

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
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }


        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            SaleQuotationHeaderViewModel vm = new SaleQuotationHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.ImportMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.ImportMenuId);

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
            return (_SaleQuotationHeaderService.GetSaleQuotationHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleQuotationHeaderService.GetSaleQuotationHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleQuotationHeaders", "SaleQuotationHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleQuotationHeaders", "SaleQuotationHeaderId", PrevNextConstants.Prev);
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //public ActionResult GeneratePrints(string Ids, int DocTypeId)
        //{

        //    if (!string.IsNullOrEmpty(Ids))
        //    {
        //        int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        //        int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

        //        var Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(DocTypeId, DivisionId, SiteId);

        //        DataTable Dt = new DataTable();
        //        String MainQuery = Settings.SqlProcDocumentPrint + " " + Ids.Split(',')[0];
        //        using (SqlConnection sqlConnection = new SqlConnection((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]))
        //        {
        //            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(MainQuery, sqlConnection);
        //            sqlDataAapter.Fill(Dt);
        //        }
        //        string Reportname = "";
        //        string path = "";
        //        if (Dt.Rows.Count > 0)
        //        {
        //            Reportname = Dt.Rows[0]["ReportName"].ToString();
        //            path = ConfigurationManager.AppSettings["PhysicalRDLCPath"] + ConfigurationManager.AppSettings["ReportsPathFromService"] + Dt.Rows[0]["ReportName"].ToString();
        //        }
        //        if (System.IO.File.Exists(path))
        //        { 
        //        string ReportSql = "";

        //        if (!string.IsNullOrEmpty(Settings.DocumentPrint))
        //            ReportSql = context.ReportHeader.Where((m) => m.ReportName == Settings.DocumentPrint).FirstOrDefault().ReportSQL;

        //        try
        //        {

        //            List<byte[]> PdfStream = new List<byte[]>();
        //            foreach (var item in Ids.Split(',').Select(Int32.Parse))
        //            {
        //                int Copies = 1;
        //                int AdditionalCopies = Settings.NoOfPrintCopies ?? 0;

        //                DirectReportPrint drp = new DirectReportPrint();

        //                var pd = context.SaleQuotationHeader.Find(item);

        //                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
        //                {
        //                    DocTypeId = pd.DocTypeId,
        //                    DocId = pd.SaleQuotationHeaderId,
        //                    ActivityType = (int)ActivityTypeContants.Print,
        //                    DocNo = pd.DocNo,
        //                    DocDate = pd.DocDate,
        //                    DocStatus = pd.Status,
        //                }));

        //                do
        //                {
        //                    byte[] Pdf;

        //                    if (!string.IsNullOrEmpty(ReportSql))
        //                    {
        //                        Pdf = drp.rsDirectDocumentPrint(ReportSql, User.Identity.Name, item);
        //                        PdfStream.Add(Pdf);
        //                    }
        //                    else
        //                    {
        //                        if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
        //                        {
        //                            //LogAct(item.ToString());
        //                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

        //                            PdfStream.Add(Pdf);
        //                        }
        //                        else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
        //                        {
        //                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

        //                            PdfStream.Add(Pdf);
        //                        }
        //                        else
        //                        {
        //                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
        //                            PdfStream.Add(Pdf);
        //                        }
        //                    }

                            

        //                    Copies--;

        //                } while (Copies > 0);

        //            }

        //            PdfMerger pm = new PdfMerger();

        //            byte[] Merge = pm.MergeFiles(PdfStream);

        //            if (Merge != null)
        //                return File(Merge, "application/pdf");

        //        }

        //        catch (Exception ex)
        //        {
        //            string message = _exception.HandleException(ex);
        //            return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
        //        }


        //        return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(new { success = "Error", data = "File Not Found. " }, JsonRequestBehavior.AllowGet);

        //    }
        //    return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);


        //}


        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {
            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(DocTypeId, DivisionId, SiteId);

                if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DocTypeId, Settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "GeneratePrints") == false)
                {
                    return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
                }

                string ReportSql = "";

                if (Settings.DocumentPrintReportHeaderId.HasValue)
                    ReportSql = db.ReportHeader.Where((m) => m.ReportHeaderId == Settings.DocumentPrintReportHeaderId).FirstOrDefault().ReportSQL;

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();
                        var pd = db.SaleQuotationHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.SaleQuotationHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (!string.IsNullOrEmpty(ReportSql))
                        {
                            Pdf = drp.rsDirectDocumentPrint(ReportSql, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }
                        else
                        {

                            if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified || pd.Status == (int)StatusConstants.Import)
                            {
                                //LogAct(item.ToString());
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                                PdfStream.Add(Pdf);
                            }
                            else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                            {
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                                PdfStream.Add(Pdf);
                            }
                            else if (pd.Status == (int)StatusConstants.Approved)
                            {
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);
                                PdfStream.Add(Pdf);
                            }

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

        public JsonResult ValidateCostCenter(int DocTypeId, int HeaderId, int JobWorkerId, string CostCenterName)
        {
            return Json(_SaleQuotationHeaderService.ValidateCostCenter(DocTypeId, HeaderId, JobWorkerId, CostCenterName), JsonRequestBehavior.AllowGet);
        }



        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";            
            int Stockline = (new SaleQuotationLineService(_unitOfWork).GetSaleQuotationLineListForIndex(id)).Count();
            if (Stockline == 0)
            {
                Msg = "Add Line Record. <br />";
            }
            else
            {
                Msg = "";
            }
            return (string.IsNullOrEmpty(Msg));
        }

        #endregion submitValidation
        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleQuotationHeaderService.GetCustomPerson(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetPersonDetail(int PersonId)
        {
            var PersonDetail = (from B in db.BusinessEntity
                                where B.PersonID == PersonId
                                select new
                                {
                                    CreditDays = B.CreaditDays ?? 0,
                                    CreditLimit = B.CreaditLimit ?? 0,
                                    SalesTaxGroupPartyId = B.SalesTaxGroupPartyId,
                                    SalesTaxGroupPartyName = B.SalesTaxGroupParty.ChargeGroupPersonName
                                }).FirstOrDefault();

            return Json(PersonDetail);
        }

    }
}
