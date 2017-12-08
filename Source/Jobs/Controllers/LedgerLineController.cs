using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Jobs.Helpers;
using Model.ViewModel;
using System.Xml.Linq;
using LedgerDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using System.Data.SqlClient;
using System.Configuration;

namespace Jobs.Controllers
{

    [Authorize]
    public class LedgerLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ILedgerService _LedgerService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public LedgerLineController(ILedgerService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _LedgerService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag(int Id)
        {
            LedgerHeader Header = new LedgerHeaderService(_unitOfWork).Find(Id);
            //ViewBag.Nature = new DocumentTypeService(_unitOfWork).Find(Header.DocTypeId).Nature;


            string Nature = "";

            if (Header.LedgerAccountId != null)
            {
                if (Header.DrCr != null)
                {
                    Nature = Header.DrCr;
                }
                else
                {
                    Nature = new DocumentTypeService(_unitOfWork).Find(Header.DocTypeId).Nature;
                }
            }
            else
            {
                Nature = "";
            }

            ViewBag.Nature = Nature ?? "";

            if (Header.LedgerAccountId != null)
            {
                ViewBag.LedgerAccountNature = new LedgerAccountService(_unitOfWork).GetLedgerAccountnature((int)Header.LedgerAccountId);
            }

            List<SelectListItem> DrCr = new List<SelectListItem>();
            DrCr.Add(new SelectListItem { Text = NatureConstants.Debit, Value = NatureConstants.Debit });
            DrCr.Add(new SelectListItem { Text = NatureConstants.Credit, Value = NatureConstants.Credit });

            ViewBag.DrCrList = new SelectList(DrCr, "Value", "Text");
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _LedgerService.GetLineListForReceiptVoucher(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult CreateLine(int Id, int? Laid, int catid, DateTime? tempduedate, string tempchequeno)
        {
            return _Create(Id, Laid, catid, tempduedate, tempchequeno, null);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int? Laid, int catid, DateTime? tempduedate, string tempchequeno)
        {
            return _Create(Id, Laid, catid, tempduedate, tempchequeno, null);
        }


        public ActionResult _Create(int Id, int? Laid, int catid, DateTime? tempduedate, string tempchequeno, int? TAID) //Id ==>Sale Order Header Id
        {

            LedgersViewModel s = new LedgersViewModel();
            //ViewBag.AccountType = new LedgerHeaderService(_unitOfWork).GetLedgerAccountType(Laid);
            LedgerHeader H = db.LedgerHeader.Find(Id);
            db.Entry<LedgerHeader>(H).Reference(m => m.LedgerAccount).Load();

            if (H.LedgerAccountId != null)
            {
                ViewBag.LedgerAccountName = H.LedgerAccount.LedgerAccountName;
            }
            else
            {
                ViewBag.LedgerAccountName = null;
            }

            PrepareViewBag(Id);
            s.DocumentCategoryId = catid;
            s.LedgerHeaderId = Id;
            s.ContraLedgerAccountId = Laid;
            s.DueDate = tempduedate;
            if (!string.IsNullOrEmpty(tempchequeno))
            {
                int id;
                int.TryParse(tempchequeno, out id);
                s.ChqNo =(id).ToString();
            }

            if (TAID.HasValue && TAID.Value > 0)
            {
                ViewBag.TAID = TAID;
            }

            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            if (settings.isVisibleReferenceDocTypeId == false)
            {
                if (settings.filterReferenceDocTypes != null)
                {
                    if (!settings.filterReferenceDocTypes.Contains(","))
                    {
                        s.ReferenceDocTypeId = Convert.ToInt32(settings.filterReferenceDocTypes);
                    }
                }
            }



            if ((settings.isVisibleLineDrCr ?? false) == true)
            {
                Decimal TotalAmountDr = 0;
                Decimal TotalAmountCr = 0;

                if (db.LedgerLine.Where(m => m.DrCr == NatureConstants.Debit && m.LedgerHeaderId == Id).Count() > 0)
                {
                    TotalAmountDr = db.LedgerLine.Where(m => m.DrCr == NatureConstants.Debit && m.LedgerHeaderId == Id).Sum(m => m.Amount);
                }

                if (db.LedgerLine.Where(m => m.DrCr == NatureConstants.Credit && m.LedgerHeaderId == Id).Count() > 0)
                {
                    TotalAmountCr = db.LedgerLine.Where(m => m.DrCr == NatureConstants.Credit && m.LedgerHeaderId == Id).Sum(m => m.Amount);
                }


                if (TotalAmountCr > TotalAmountDr)
                {
                    s.DrCr = NatureConstants.Debit;
                }
                if (TotalAmountDr > TotalAmountCr)
                {
                    s.DrCr = NatureConstants.Credit;
                }
            }




            var LedgerLine = (from p in db.LedgerLine
                              where p.LedgerHeaderId == Id
                              orderby p.LedgerLineId descending
                              select new
                              {
                                  Name = p.LedgerAccount.LedgerAccountName,
                                  CostCenterName = p.CostCenter.CostCenterName,
                                  Amount = p.Amount,
                              }).FirstOrDefault();

            if (LedgerLine != null)
            {
                ViewBag.LastTransaction = "Last Line -Name : " + LedgerLine.Name + (LedgerLine.CostCenterName != null ? ", " + "Cost Center : " + LedgerLine.CostCenterName : "") + ", " + "Amount : " + LedgerLine.Amount.ToString("0.00");
            }
            else
            {
                ViewBag.LastTransaction = "";
            }

            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(LedgersViewModel svm)
        {
            LedgerHeader header = new LedgerHeaderService(_unitOfWork).Find(svm.LedgerHeaderId);
            //string Nature = new DocumentTypeService(_unitOfWork).Find(header.DocTypeId).Nature;
            string Nature = "";

            if (svm.DrCr == null)
            {
                if (header.DrCr != null)
                {
                    Nature = header.DrCr;
                }
                else
                {
                    Nature = new DocumentTypeService(_unitOfWork).Find(header.DocTypeId).Nature;
                }
            }
            else
            {
                Nature = svm.DrCr;
            }


            //Ledger line = Mapper.Map<LedgersViewModel, Ledger>(svm);

            if (svm.LedgerLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.LedgerSetting != null)
            {
                if (svm.LedgerSetting.isVisibleChqNo && svm.LedgerSetting.isMandatoryChqNo == true && (string.IsNullOrEmpty(svm.ChqNo)))
                {
                    ModelState.AddModelError("ChqNo", "The ChqNo field is required");
                }
                if (svm.LedgerSetting.isVisibleLineCostCenter == true && svm.LedgerSetting.isMandatoryLineCostCenter == true && !svm.CostCenterId.HasValue)
                {
                    ModelState.AddModelError("CostCenterId", "The CostCenter field is required");
                }
                if (svm.LedgerSetting.isVisibleRefNo == true && svm.LedgerSetting.isMandatoryRefNo == true && !svm.ReferenceId.HasValue)
                {
                    ModelState.AddModelError("ReferenceId", "The Reference No field is required");
                }
                if (svm.LedgerSetting.isVisibleLineDrCr && (string.IsNullOrEmpty(svm.DrCr)))
                {
                    ModelState.AddModelError("DrCr", "The DrCr field is required");
                }
            }

            bool BeforeSave = true;
            try
            {

                if (svm.LedgerHeaderId <= 0)
                    BeforeSave = LedgerDocEvents.beforeLineSaveEvent(this, new LedgerEventArgs(svm.LedgerHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = LedgerDocEvents.beforeLineSaveEvent(this, new LedgerEventArgs(svm.LedgerHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (svm.LedgerLineId <= 0)
                {
                    LedgerLine LedgerLine = new LedgerLine();

                    LedgerLine.LedgerHeaderId = header.LedgerHeaderId;
                    LedgerLine.LedgerAccountId = svm.LedgerAccountId;
                    LedgerLine.Amount = svm.Amount;
                    LedgerLine.ChqNo = svm.ChqNo;
                    LedgerLine.ChqDate = svm.DueDate;
                    LedgerLine.CostCenterId = svm.CostCenterId;
                    LedgerLine.BaseRate = svm.BaseRate;
                    LedgerLine.BaseValue = svm.BaseValue;
                    LedgerLine.ReferenceId = svm.ReferenceId;
                    LedgerLine.ProductUidId = svm.ProductUidId;
                    LedgerLine.ReferenceDocTypeId = svm.ReferenceDocTypeId;
                    LedgerLine.ReferenceDocId = svm.ReferenceDocId;
                    LedgerLine.DrCr = svm.DrCr;
                    LedgerLine.CreatedDate = DateTime.Now;
                    LedgerLine.ModifiedDate = DateTime.Now;
                    LedgerLine.CreatedBy = User.Identity.Name;
                    LedgerLine.Remark = svm.Remark;
                    LedgerLine.ModifiedBy = User.Identity.Name;
                    LedgerLine.ObjectState = Model.ObjectState.Added;
                    db.LedgerLine.Add(LedgerLine);
                    //new LedgerLineService(_unitOfWork).Create(LedgerLine);


                    var FromCosterName = db.CostCenter.Find(header.CostCenterId);
                    var ToCosterName = db.CostCenter.Find(svm.CostCenterId);

                    if (header.DocTypeId == 286)
                    {
                        header.Narration = header.Narration + " Form Costcenter " + FromCosterName.CostCenterName + " To " + ToCosterName.CostCenterName;
                    }

                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        //new LedgerHeaderService(_unitOfWork).Update(header);
                        header.ObjectState = Model.ObjectState.Modified;
                        db.LedgerHeader.Add(header);
                    }

                    if (LedgerLine.CostCenterId.HasValue && LedgerLine.CostCenterId.Value != 0)
                    {
                        var CostCenterStatus = db.CostCenterStatus.Find(LedgerLine.CostCenterId);

                        if (Nature == NatureConstants.Debit)
                        {
                            CostCenterStatus.AmountCr = (CostCenterStatus.AmountCr ?? 0) + LedgerLine.Amount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(CostCenterStatus);
                            if (header.CostCenterId.HasValue && header.CostCenterId.Value != 0)
                            {
                                var HeaderCostCenterStatus = db.CostCenterStatus.Find(header.CostCenterId);
                                HeaderCostCenterStatus.AmountDr = (HeaderCostCenterStatus.AmountDr ?? 0) + LedgerLine.Amount;
                                HeaderCostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatus.Add(HeaderCostCenterStatus);
                            }
                        }
                        else
                        {
                            CostCenterStatus.AmountDr = (CostCenterStatus.AmountDr ?? 0) + LedgerLine.Amount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(CostCenterStatus);
                            if (header.CostCenterId.HasValue && header.CostCenterId.Value != 0)
                            {
                                var HeaderCostCenterStatus = db.CostCenterStatus.Find(header.CostCenterId);
                                HeaderCostCenterStatus.AmountCr = (HeaderCostCenterStatus.AmountCr ?? 0) + LedgerLine.Amount;
                                HeaderCostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatus.Add(HeaderCostCenterStatus);
                            }
                        }
                    }


                    if (header.LedgerAccountId != null)
                    {
                        #region LedgerSave
                        Ledger Ledger = new Ledger();

                        if (Nature == NatureConstants.Credit)
                        {
                            Ledger.AmtDr = LedgerLine.Amount;

                        }
                        else if (Nature == NatureConstants.Debit)
                        {
                            Ledger.AmtCr = LedgerLine.Amount;
                        }
                        Ledger.ChqNo = LedgerLine.ChqNo;
                        Ledger.ChqDate = LedgerLine.ChqDate;
                        Ledger.ContraLedgerAccountId = header.LedgerAccountId;
                        Ledger.CostCenterId = LedgerLine.CostCenterId;
                        Ledger.DueDate = LedgerLine.ChqDate;
                        Ledger.LedgerAccountId = LedgerLine.LedgerAccountId;
                        Ledger.LedgerHeaderId = LedgerLine.LedgerHeaderId;
                        Ledger.LedgerLineId = LedgerLine.LedgerLineId;
                        Ledger.ProductUidId = LedgerLine.ProductUidId;
                        Ledger.Narration = header.Narration + LedgerLine.Remark;
                        Ledger.ObjectState = Model.ObjectState.Added;
                        Ledger.LedgerId = 1;
                        db.Ledger.Add(Ledger);

                        if (LedgerLine.ReferenceId != null)
                        {
                            LedgerAdj LedgerAdj = new LedgerAdj();
                            if (Nature == NatureConstants.Credit)
                            {
                                //LedgerAdj.LedgerId = (int)LedgerLine.ReferenceId;
                                LedgerAdj.DrLedgerId = Ledger.LedgerId;
                                LedgerAdj.CrLedgerId = (int)LedgerLine.ReferenceId;
                            }
                            else
                            {
                                //LedgerAdj.LedgerId = (int)LedgerLine.ReferenceId;
                                LedgerAdj.CrLedgerId = Ledger.LedgerId;
                                LedgerAdj.DrLedgerId = (int)LedgerLine.ReferenceId;
                            }

                            LedgerAdj.Amount = LedgerLine.Amount;
                            LedgerAdj.SiteId = header.SiteId;
                            LedgerAdj.CreatedDate = DateTime.Now;
                            LedgerAdj.ModifiedDate = DateTime.Now;
                            LedgerAdj.CreatedBy = User.Identity.Name;
                            LedgerAdj.ModifiedBy = User.Identity.Name;
                            LedgerAdj.ObjectState = Model.ObjectState.Added;
                            db.LedgerAdj.Add(LedgerAdj);
                        }
                        #endregion

                        #region ContraLedgerSave
                        Ledger ContraLedger = new Ledger();
                        if (Nature == NatureConstants.Credit)
                        {
                            ContraLedger.AmtCr = LedgerLine.Amount;

                        }
                        else if (Nature == NatureConstants.Debit)
                        {
                            ContraLedger.AmtDr = LedgerLine.Amount;
                        }
                        ContraLedger.LedgerHeaderId = header.LedgerHeaderId;
                        ContraLedger.CostCenterId = header.CostCenterId;
                        ContraLedger.LedgerLineId = LedgerLine.LedgerLineId;
                        ContraLedger.LedgerAccountId = header.LedgerAccountId.Value;
                        ContraLedger.ContraLedgerAccountId = LedgerLine.LedgerAccountId;
                        ContraLedger.ChqNo = LedgerLine.ChqNo;
                        ContraLedger.ChqDate = LedgerLine.ChqDate;
                        ContraLedger.ObjectState = Model.ObjectState.Added;
                        db.Ledger.Add(ContraLedger);
                        #endregion
                    }



                    try
                    {
                        LedgerDocEvents.onLineSaveEvent(this, new LedgerEventArgs(LedgerLine.LedgerHeaderId, LedgerLine.LedgerLineId, EventModeConstants.Add), ref db);
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
                        PrepareViewBag(header.LedgerHeaderId);
                        ViewBag.AccountType = new LedgerHeaderService(_unitOfWork).GetLedgerAccountType(svm.ContraLedgerAccountId ?? 0);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        LedgerDocEvents.afterLineSaveEvent(this, new LedgerEventArgs(LedgerLine.LedgerHeaderId, LedgerLine.LedgerLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.LedgerHeaderId,
                        DocLineId = LedgerLine.LedgerLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));

                    return RedirectToAction("_Create", new { id = LedgerLine.LedgerHeaderId, Laid = svm.ContraLedgerAccountId, catid = svm.DocumentCategoryId, tempchequeno = svm.ChqNo, TAID = LedgerLine.LedgerAccountId });
                }


                else //Edit Mode 
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    int status = header.Status;

                    LedgerLine LedgerLine = new LedgerLineService(_unitOfWork).Find(svm.LedgerLineId);
                    int XLedgerAccountId = LedgerLine.LedgerAccountId;
                    LedgerLine ExRec = Mapper.Map<LedgerLine>(LedgerLine);

                    LedgerLine.LedgerAccountId = svm.LedgerAccountId;
                    LedgerLine.Amount = svm.Amount;
                    LedgerLine.ChqNo = svm.ChqNo;
                    LedgerLine.CostCenterId = svm.CostCenterId;
                    LedgerLine.Remark = svm.Remark;
                    LedgerLine.ChqDate = svm.DueDate;
                    LedgerLine.BaseRate = svm.BaseRate;
                    LedgerLine.BaseValue = svm.BaseValue;
                    LedgerLine.ReferenceId = svm.ReferenceId;
                    LedgerLine.ProductUidId = svm.ProductUidId;
                    LedgerLine.DrCr = svm.DrCr;
                    LedgerLine.ReferenceDocTypeId = svm.ReferenceDocTypeId;
                    LedgerLine.ReferenceDocId = svm.ReferenceDocId;
                    LedgerLine.ModifiedDate = DateTime.Now;
                    LedgerLine.ModifiedBy = User.Identity.Name;
                    LedgerLine.ObjectState = Model.ObjectState.Modified;
                    db.LedgerLine.Add(LedgerLine);
                    //new LedgerLineService(_unitOfWork).Update(LedgerLine);

                    var FromCosterName = db.CostCenter.Find(header.CostCenterId);
                    var ToCosterName = db.CostCenter.Find(svm.CostCenterId);

                    if (header.DocTypeId == 286)
                    {
                        header.Narration = header.Narration + " Form Costcenter " + FromCosterName.CostCenterName + " To " + ToCosterName.CostCenterName;
                    }

                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedDate = DateTime.Now;
                        header.ModifiedBy = User.Identity.Name;
                        header.ObjectState = Model.ObjectState.Modified;
                        db.LedgerHeader.Add(header);
                        //new LedgerHeaderService(_unitOfWork).Update(header);
                    }


                    if (LedgerLine.CostCenterId.HasValue && LedgerLine.CostCenterId.Value != 0)
                    {


                        var CostCenterStatus = db.CostCenterStatus.Find(LedgerLine.CostCenterId);

                        if (Nature == NatureConstants.Debit)
                        {

                            CostCenterStatus.AmountCr = ((CostCenterStatus.AmountCr ?? 0) - ExRec.Amount) + LedgerLine.Amount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(CostCenterStatus);
                            if (header.CostCenterId.HasValue && header.CostCenterId.Value != 0)
                            {
                                var HeaderCostCenterStatus = db.CostCenterStatus.Find(header.CostCenterId);
                                HeaderCostCenterStatus.AmountDr = ((HeaderCostCenterStatus.AmountDr ?? 0) - ExRec.Amount) + LedgerLine.Amount;
                                HeaderCostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatus.Add(HeaderCostCenterStatus);
                            }
                        }
                        else
                        {

                            CostCenterStatus.AmountDr = ((CostCenterStatus.AmountDr ?? 0) - ExRec.Amount) + LedgerLine.Amount;
                            CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(CostCenterStatus);
                            if (header.CostCenterId.HasValue && header.CostCenterId.Value != 0)
                            {
                                var HeaderCostCenterStatus = db.CostCenterStatus.Find(header.CostCenterId);
                                HeaderCostCenterStatus.AmountCr = ((HeaderCostCenterStatus.AmountCr ?? 0) - ExRec.Amount) + LedgerLine.Amount;
                                HeaderCostCenterStatus.ObjectState = Model.ObjectState.Modified;
                                db.CostCenterStatus.Add(HeaderCostCenterStatus);
                            }
                        }
                    }

                    if (header.LedgerAccountId != null)
                    {
                        #region LedgerSave
                        Ledger Ledger = new Ledger();

                        if (Nature == NatureConstants.Credit)
                        {
                            Ledger = db.Ledger.Where(m => m.LedgerLineId == LedgerLine.LedgerLineId && m.LedgerAccountId == XLedgerAccountId && m.AmtDr > 0).FirstOrDefault();
                            Ledger.AmtDr = LedgerLine.Amount;
                        }
                        else if (Nature == NatureConstants.Debit)
                        {
                            Ledger = db.Ledger.Where(m => m.LedgerLineId == LedgerLine.LedgerLineId && m.LedgerAccountId == XLedgerAccountId && m.AmtCr > 0).FirstOrDefault();
                            Ledger.AmtCr = LedgerLine.Amount;
                        }
                        Ledger.ChqNo = LedgerLine.ChqNo;
                        Ledger.ContraLedgerAccountId = header.LedgerAccountId;
                        Ledger.CostCenterId = LedgerLine.CostCenterId;
                        Ledger.DueDate = LedgerLine.ChqDate;
                        Ledger.LedgerAccountId = LedgerLine.LedgerAccountId;
                        Ledger.LedgerHeaderId = LedgerLine.LedgerHeaderId;
                        Ledger.LedgerLineId = LedgerLine.LedgerLineId;
                        Ledger.Narration = header.Narration + LedgerLine.Remark;
                        Ledger.ProductUidId = LedgerLine.ProductUidId;
                        Ledger.ObjectState = Model.ObjectState.Modified;
                        db.Ledger.Add(Ledger);

                        if (LedgerLine.ReferenceId != null)
                        {
                            LedgerAdj LedgerAdj = new LedgerAdj();

                            if (Nature == NatureConstants.Credit)
                            {
                                LedgerAdj = db.LedgerAdj.Where(m => m.DrLedgerId == Ledger.LedgerId && m.CrLedgerId == LedgerLine.ReferenceId).FirstOrDefault();
                                //LedgerAdj.LedgerId = (int)LedgerLine.ReferenceId;
                                LedgerAdj.DrLedgerId = Ledger.LedgerId;
                                LedgerAdj.CrLedgerId = (int)LedgerLine.ReferenceId;
                            }
                            else
                            {
                                LedgerAdj = db.LedgerAdj.Where(m => m.CrLedgerId == Ledger.LedgerId && m.DrLedgerId == LedgerLine.ReferenceId).FirstOrDefault();
                                //LedgerAdj.LedgerId = (int)LedgerLine.ReferenceId;
                                LedgerAdj.CrLedgerId = Ledger.LedgerId;
                                LedgerAdj.DrLedgerId = (int)LedgerLine.ReferenceId;
                            }

                            LedgerAdj.Amount = LedgerLine.Amount;
                            LedgerAdj.SiteId = header.SiteId;
                            LedgerAdj.CreatedDate = DateTime.Now;
                            LedgerAdj.ModifiedDate = DateTime.Now;
                            LedgerAdj.CreatedBy = User.Identity.Name;
                            LedgerAdj.ModifiedBy = User.Identity.Name;
                            LedgerAdj.ObjectState = Model.ObjectState.Modified;
                            db.LedgerAdj.Add(LedgerAdj);
                        }
                        #endregion

                        #region ContraLedgerSave
                        Ledger ContraLedger = new Ledger();
                        if (Nature == NatureConstants.Credit)
                        {
                            ContraLedger = db.Ledger.Where(m => m.LedgerLineId == LedgerLine.LedgerLineId && m.LedgerAccountId == header.LedgerAccountId && m.AmtCr > 0).FirstOrDefault();
                            ContraLedger.AmtCr = LedgerLine.Amount;

                        }
                        else if (Nature == NatureConstants.Debit)
                        {
                            ContraLedger = db.Ledger.Where(m => m.LedgerLineId == LedgerLine.LedgerLineId && m.LedgerAccountId == header.LedgerAccountId && m.AmtDr > 0).FirstOrDefault();
                            ContraLedger.AmtDr = LedgerLine.Amount;
                        }
                        ContraLedger.LedgerHeaderId = header.LedgerHeaderId;
                        ContraLedger.CostCenterId = header.CostCenterId;
                        ContraLedger.LedgerLineId = LedgerLine.LedgerLineId;
                        ContraLedger.LedgerAccountId = header.LedgerAccountId.Value;
                        ContraLedger.ContraLedgerAccountId = LedgerLine.LedgerAccountId;
                        ContraLedger.ObjectState = Model.ObjectState.Modified;
                        db.Ledger.Add(ContraLedger);
                        #endregion
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = LedgerLine,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        LedgerDocEvents.onLineSaveEvent(this, new LedgerEventArgs(LedgerLine.LedgerHeaderId, LedgerLine.LedgerLineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(header.LedgerHeaderId);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        LedgerDocEvents.afterLineSaveEvent(this, new LedgerEventArgs(LedgerLine.LedgerHeaderId, LedgerLine.LedgerLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = LedgerLine.LedgerHeaderId,
                        DocLineId = LedgerLine.LedgerLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = header.DocNo,
                        xEModifications = Modifications,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));

                    return Json(new { success = true });

                }
            }
            PrepareViewBag(header.LedgerHeaderId);
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
        private ActionResult _Modify(int id)
        {
            LedgersViewModel temp = _LedgerService.GetLedgerVm(id);

            

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

            LedgerHeader H = db.LedgerHeader.Find(temp.LedgerHeaderId);

            db.Entry<LedgerHeader>(H).Reference(m => m.LedgerAccount).Load();

            if (H.LedgerAccountId != null)
            {
                ViewBag.LedgerAccountName = H.LedgerAccount.LedgerAccountName;
                temp.ContraLedgerAccountId = H.LedgerAccountId;
            }
            else
            {
                ViewBag.LedgerAccountName = null;
            }

            PrepareViewBag(temp.LedgerHeaderId);
            //ViewBag.AccountType = new LedgerHeaderService(_unitOfWork).GetLedgerAccountType(temp.ContraLedgerAccountId ?? 0);
            //Getting Settings
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            temp.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);

            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            LedgersViewModel temp = _LedgerService.GetLedgerVm(id);
            if (temp == null)
            {
                return HttpNotFound();
            }

            LedgerHeader H = new LedgerHeaderService(_unitOfWork).Find(temp.LedgerHeaderId);

            PrepareViewBag(temp.LedgerHeaderId);
            ViewBag.AccountType = new LedgerHeaderService(_unitOfWork).GetLedgerAccountType(temp.ContraLedgerAccountId ?? 0);
            //Getting Settings
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);

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


        private ActionResult _Delete(int id)
        {
            LedgersViewModel temp = _LedgerService.GetLedgerVm(id);

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

            LedgerHeader H = new LedgerHeaderService(_unitOfWork).Find(temp.LedgerHeaderId);

            PrepareViewBag(temp.LedgerHeaderId);
            ViewBag.AccountType = new LedgerHeaderService(_unitOfWork).GetLedgerAccountType(temp.ContraLedgerAccountId ?? 0);

            //Getting Settings
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(LedgersViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = LedgerDocEvents.beforeLineDeleteEvent(this, new LedgerEventArgs(vm.LedgerHeaderId, vm.LedgerLineId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                LedgerHeader header = db.LedgerHeader.Find(vm.LedgerHeaderId);
                //string Nature = new DocumentTypeService(_unitOfWork).Find(header.DocTypeId).Nature;

                string Nature = "";

                if (vm.DrCr == null)
                {
                    if (header.DrCr != null)
                    {
                        Nature = header.DrCr;
                    }
                    else
                    {
                        Nature = new DocumentTypeService(_unitOfWork).Find(header.DocTypeId).Nature;
                    }
                }
                else
                {
                    Nature = vm.DrCr;
                }

                int LedgerLineId = vm.LedgerLineId;
                LedgerLine Line = db.LedgerLine.Find(LedgerLineId);
                int? CostCenterId = Line.CostCenterId;


                List<Ledger> Ledgers = db.Ledger.Where(m => m.LedgerLineId == LedgerLineId).ToList();

                var LedgerIds = Ledgers.Select(m => m.LedgerId).ToArray();

                //LedgerAdj LedgerAdjs = new LedgerAdj();

                //if (Nature == NatureConstants.Credit)
                //{
                //    var crLedger = Ledgers.Where(m => m.LedgerLineId == Line.LedgerLineId && m.LedgerAccountId == Line.LedgerAccountId && m.AmtDr > 0).FirstOrDefault();
                //    LedgerAdjs = db.LedgerAdj.Where(m => m.DrLedgerId == crLedger.LedgerId && m.LedgerId == Line.ReferenceId).FirstOrDefault();
                //}
                //else
                //{
                //    var drLedger = Ledgers.Where(m => m.LedgerLineId == Line.LedgerLineId && m.LedgerAccountId == Line.LedgerAccountId && m.AmtCr > 0).FirstOrDefault();
                //    LedgerAdjs = db.LedgerAdj.Where(m => m.CrLedgerId == drLedger.LedgerId && m.LedgerId == Line.ReferenceId).FirstOrDefault();
                //}

                //if (Line.ReferenceId.HasValue && Line.ReferenceId.Value > 0)
                //{
                //    LedgerAdjs.ObjectState = Model.ObjectState.Deleted;
                //    db.LedgerAdj.Remove(LedgerAdjs);
                //}

                foreach (var item in Ledgers)
                {
                    var LedgerAdjList = (from L in db.LedgerAdj where L.DrLedgerId == item.LedgerId || L.CrLedgerId == item.LedgerId select L).ToList();
                    foreach(var LedgerAdj in LedgerAdjList)
                    {
                        LedgerAdj.ObjectState = Model.ObjectState.Deleted;
                        db.LedgerAdj.Remove(LedgerAdj);
                    }
                }

                foreach (var item in Ledgers)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.Ledger.Remove(item);
                }


                if (LedgerLineId != 0)
                {
                    var RefLines = (from p in db.LedgerLineRefValue
                                    where p.LedgerLineId == LedgerLineId
                                    select p).ToList();

                    foreach (var item in RefLines)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.LedgerLineRefValue.Remove(item);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<LedgerLine>(Line)
                    });

                    Line.ObjectState = Model.ObjectState.Deleted;
                    db.LedgerLine.Remove(Line);
                }


                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new LedgerHeaderService(_unitOfWork).Update(header);
                    header.ObjectState = Model.ObjectState.Modified;
                    db.LedgerHeader.Add(header);
                }

                if (CostCenterId.HasValue && CostCenterId.Value != 0)
                {
                    var CostCenterStatus = db.CostCenterStatus.Find(CostCenterId);

                    if (Nature == NatureConstants.Debit)
                    {
                        CostCenterStatus.AmountCr = (CostCenterStatus.AmountCr ?? 0) - Line.Amount;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatus.Add(CostCenterStatus);
                        if (header.CostCenterId.HasValue && header.CostCenterId.Value != 0)
                        {
                            var HeaderCostCenterStatus = db.CostCenterStatus.Find(header.CostCenterId);
                            HeaderCostCenterStatus.AmountDr = (HeaderCostCenterStatus.AmountDr ?? 0) - Line.Amount;
                            HeaderCostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(HeaderCostCenterStatus);
                        }
                    }
                    else
                    {
                        CostCenterStatus.AmountDr = (CostCenterStatus.AmountDr ?? 0) - Line.Amount;
                        CostCenterStatus.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatus.Add(CostCenterStatus);
                        if (header.CostCenterId.HasValue && header.CostCenterId.Value != 0)
                        {
                            var HeaderCostCenterStatus = db.CostCenterStatus.Find(header.CostCenterId);
                            HeaderCostCenterStatus.AmountCr = (HeaderCostCenterStatus.AmountCr ?? 0) - Line.Amount;
                            HeaderCostCenterStatus.ObjectState = Model.ObjectState.Modified;
                            db.CostCenterStatus.Add(HeaderCostCenterStatus);
                        }
                    }
                }



                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    LedgerDocEvents.onLineDeleteEvent(this, new LedgerEventArgs(Line.LedgerHeaderId, Line.LedgerLineId), ref db);
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
                        throw new Exception();
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    ViewBag.LineMode = "Delete";
                    PrepareViewBag(header.LedgerHeaderId);
                    return PartialView("_Create", vm);
                }

                try
                {
                    LedgerDocEvents.afterLineDeleteEvent(this, new LedgerEventArgs(Line.LedgerHeaderId, Line.LedgerLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.LedgerHeaderId,
                    DocLineId = vm.LedgerLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
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

        public JsonResult GetPersonPendingBills(int LedgerHeaderId, int LedgerAccountId, string ReferenceType, string term, int Limit)
        {
            List<LedgerList> LedgerList = new LedgerLineService(_unitOfWork).GetPersonPendingBills(LedgerHeaderId, LedgerAccountId, ReferenceType, term, Limit).ToList();

            return Json(LedgerList);
        }

        public JsonResult GetLedgerAcc(int CostCenterId)
        {
            CostCenter Cs = new CostCenterService(_unitOfWork).Find(CostCenterId);
            if (Cs.LedgerAccountId.HasValue)
            {
                LedgerAccount LA = new LedgerAccountService(_unitOfWork).Find(Cs.LedgerAccountId.Value);

                return Json(new { Success = true, Name = LA.LedgerAccountName, Id = LA.LedgerAccountId });
            }
            else
            {
                return Json(new { Success = false });
            }

        }

        public JsonResult FetchRate(int PersonId)
        {
            if (PersonId != 0)
            {
                LedgerAccount LA = new LedgerAccountService(_unitOfWork).Find(PersonId);
                decimal Rate = 0;

                var TdsRate = (from p in db.BusinessEntity
                               where p.PersonID == LA.PersonId
                               let TDSGroupId = p.TdsGroupId
                               let TDSCatId = p.TdsCategoryId
                               from T in db.TdsRate
                               where T.TdsCategoryId == TDSCatId && T.TdsGroupId == TDSGroupId
                               select T).FirstOrDefault();
                if (TdsRate != null)
                {
                    Rate = TdsRate.Percentage;
                }

                return Json(new { Success = true, Rate = Rate });
            }


            return Json(new { Success = true, Rate = 0 });
        }


        public JsonResult GetLedgerAccount(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {

            LedgerHeader Ledger = new LedgerHeaderService(_unitOfWork).Find(filter);

            var Settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(Ledger.DocTypeId, Ledger.DivisionId, Ledger.SiteId);

            var Query = new LedgerLineService(_unitOfWork).GetLedgerAccounts(searchTerm, Settings.filterLedgerAccountGroupLines, Settings.filterExcludeLedgerAccountGroupLines, (Ledger.ProcessId.HasValue ? Ledger.ProcessId.ToString() : Settings.filterPersonProcessLines));

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

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

        public JsonResult GetCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {

            LedgerHeader Ledger = new LedgerHeaderService(_unitOfWork).Find(filter);

            var Settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(Ledger.DocTypeId, Ledger.DivisionId, Ledger.SiteId);

            var temp = new LedgerLineService(_unitOfWork).GetCostCenters(searchTerm, Settings.filterDocTypeCostCenter, Settings.filterPersonProcessLines).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new LedgerLineService(_unitOfWork).GetCostCenters(searchTerm, Settings.filterDocTypeCostCenter, Settings.filterPersonProcessLines).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetProductUidValidation(string ProductUID)
        {
            return Json(new ProductUidService(_unitOfWork).ValidateUID(ProductUID), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReferenceDocIds(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = new LedgerLineService(_unitOfWork).GetReferenceDocIds(filter, searchTerm);
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


        public JsonResult SetSingleReferenceDocIds(int Ids, int ReferenceDocTypeId)
        {
             var temp = new LedgerLineService(_unitOfWork).SetReferenceDocIds(Ids, ReferenceDocTypeId);

            ComboBoxResult ReferenceDocIdJson = new ComboBoxResult();

            ReferenceDocIdJson.id = temp.id;
            ReferenceDocIdJson.text = temp.text;

            return Json(ReferenceDocIdJson);
        }

        public JsonResult GetLastTransactionDetailJson(int LedgerHeaderId)
        {
            var LastTransactionDetail = new LedgerLineService(_unitOfWork).GetLastTransactionDetail(LedgerHeaderId);
            return Json(LastTransactionDetail);
        }


        [HttpGet]
        public ActionResult _LedgerAdj(int id)
        {
            LedgerLine Line = new LedgerLineService(_unitOfWork).Find(id);
            LedgerHeader Header = new LedgerHeaderService(_unitOfWork).Find(Line.LedgerHeaderId);
            LedgerToAdjustViewModel s = new LedgerToAdjustViewModel();

            var temp = (from L in db.Ledger
                        where L.LedgerLineId == id && L.LedgerAccountId == Line.LedgerAccountId
                        select new
                        {
                            LedgerId = L.LedgerId,
                            Amount = L.AmtDr > 0 ? L.AmtDr : L.AmtCr,
                            DrCr = L.AmtDr > 0 ? NatureConstants.Debit : NatureConstants.Credit
                        }).FirstOrDefault();

            if (temp != null)
            {
                s.LedgerId = temp.LedgerId;
                s.Amount = temp.Amount;
                s.DrCr = temp.DrCr;
            }

            //var PendingLedgerViewModel = (from L in db.ViewLedgerBalance
            //                              join H in db.LedgerHeader on L.LedgerHeaderId equals H.LedgerHeaderId into LedgerHeaderTable
            //                              from LedgerHeaderTab in LedgerHeaderTable.DefaultIfEmpty()
            //                              orderby LedgerHeaderTab.DocDate
            //                              where L.LedgerAccountId == Line.LedgerAccountId
            //                              select new PendingLedgerViewModel
            //                              {
            //                                  LedgerId = L.LedgerId,
            //                                  LedgerHeaderDocNo = LedgerHeaderTab.DocNo,
            //                                  LedgerHeaderDocDate = LedgerHeaderTab.DocDate,
            //                                  PartyDocNo = LedgerHeaderTab.PartyDocNo,
            //                                  PartyDocDate = LedgerHeaderTab.PartyDocDate,
            //                                  BillAmount = L.Balance
            //                              }).ToList();

            SqlParameter SqlParameterLedgerId = new SqlParameter("@LedgerId", temp.LedgerId);
            var PendingLedgerViewModel = db.Database.SqlQuery<PendingLedgerViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".sp_GetLedgerToAdjust @LedgerId", SqlParameterLedgerId).ToList();

            if (PendingLedgerViewModel.Sum( m => m.AdjustedAmount) == 0)
            {
                Decimal PendingToAdjustAmount = s.Amount;
                foreach (var item in PendingLedgerViewModel)
                {
                    if (item.BalanceAmount <= PendingToAdjustAmount)
                    {
                        item.IsSelected = true;
                        item.AdjustedAmount = item.BalanceAmount;
                        PendingToAdjustAmount = PendingToAdjustAmount - item.BalanceAmount;
                    }
                    else if (item.BalanceAmount > PendingToAdjustAmount && PendingToAdjustAmount > 0)
                    {
                        item.IsSelected = true;
                        item.AdjustedAmount = PendingToAdjustAmount;
                        PendingToAdjustAmount = 0;
                    }
                    else
                    {
                        item.IsSelected = false;
                        item.AdjustedAmount = 0;
                    }
                }
            }


            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            s.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);

            s.LedgerViewModel = PendingLedgerViewModel;

            ViewBag.Name = new LedgerAccountService(_unitOfWork).Find(Line.LedgerAccountId).LedgerAccountName + "  " + "Amount : " + Line.Amount.ToString();


            if (s == null)
            {
                return HttpNotFound();
            }
            return PartialView("_LedgerAdj", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _LedgerAdj(LedgerToAdjustViewModel svm)
        {
            if (ModelState.IsValid)
            {
                IEnumerable<LedgerAdj> LedgerAdjList = null;

                if (svm.DrCr == NatureConstants.Debit)
                {
                    LedgerAdjList = (from L in db.LedgerAdj where L.DrLedgerId == svm.LedgerId select L).ToList();
                }
                else
                {
                    LedgerAdjList = (from L in db.LedgerAdj where L.CrLedgerId == svm.LedgerId select L).ToList();
                }

                if (LedgerAdjList != null)
                {
                    foreach (var item in LedgerAdjList)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.LedgerAdj.Add(item);
                    }
                }


                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


                foreach (var item in svm.LedgerViewModel)
                {
                    if (item.AdjustedAmount != 0 && item.AdjustedAmount != null)
                    {
                        LedgerAdj Adj = new LedgerAdj();
                        if (svm.DrCr == NatureConstants.Debit)
                        {
                            Adj.DrLedgerId = svm.LedgerId;
                            Adj.CrLedgerId = item.LedgerId;
                        }
                        else
                        {
                            Adj.CrLedgerId = svm.LedgerId;
                            Adj.DrLedgerId = item.LedgerId;
                        }

                        Adj.Amount = item.AdjustedAmount ?? 0;
                        Adj.SiteId = SiteId;
                        Adj.CreatedDate = DateTime.Now;
                        Adj.ModifiedDate = DateTime.Now;
                        Adj.CreatedBy = User.Identity.Name;
                        Adj.ModifiedBy = User.Identity.Name;
                        Adj.ObjectState = Model.ObjectState.Added;
                        db.LedgerAdj.Add(Adj);
                    }
                }

                try
                {
                    //_unitOfWork.Save();
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_LedgerAdj_Single", svm);
                }

                return Json(new { success = true });

            }

            return PartialView("_LedgerAdj", svm);
        }


        [HttpGet]
        public ActionResult _LedgerAdj_Single(int id)
        {
            LedgerLine Line = new LedgerLineService(_unitOfWork).Find(id);
            LedgerHeader Header = new LedgerHeaderService(_unitOfWork).Find(Line.LedgerHeaderId);
            LedgerToAdjustViewModel_Single s = new LedgerToAdjustViewModel_Single();

            s.LedgerLineId = id;
            s.LedgerAccountId = Line.LedgerAccountId;
            s.LedgerHeaderId = Line.LedgerHeaderId;
            s.PartyDocDate_Adjusted = null;

            var temp = (from L in db.Ledger
                        join Ld in db.ViewLedgerBalance on L.LedgerId equals Ld.LedgerId into LedgerBalanceTable from LedgerBalanceTab in LedgerBalanceTable.DefaultIfEmpty()
                        where L.LedgerLineId == id && L.LedgerAccountId == Line.LedgerAccountId
                        select new
                        {
                            LedgerId = L.LedgerId,
                            Amount = L.AmtDr > 0 ? L.AmtDr : L.AmtCr,
                            BalanceAmount = (Decimal?)LedgerBalanceTab.Balance ?? 0,
                            DrCr = L.AmtDr > 0 ? NatureConstants.Debit : NatureConstants.Credit
                        }).FirstOrDefault();

            if (temp != null)
            {
                s.LedgerId = temp.LedgerId;
                s.Amount = temp.Amount;
                s.BalanceAmount = temp.BalanceAmount;
                s.DrCr = temp.DrCr;
            }
            
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            s.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);



            List<PendingLedgerViewModel> LedgerAdjustedList = null;
            if (s.DrCr == NatureConstants.Debit)
            {
                LedgerAdjustedList = (from L in db.LedgerAdj
                                      join Ld in db.Ledger on L.CrLedgerId equals Ld.LedgerId into LedgerTable
                                      from LedgerTab in LedgerTable.DefaultIfEmpty()
                                      join H in db.LedgerHeader on LedgerTab.LedgerHeaderId equals H.LedgerHeaderId into LedgerHeaderTable
                                      from LedgerHeaderTab in LedgerHeaderTable.DefaultIfEmpty()
                                      where L.DrLedgerId == s.LedgerId
                                      select new PendingLedgerViewModel
                                      {
                                          LedgerAdjId = L.LedgerAdjId,
                                          LedgerId = LedgerTab.LedgerId,
                                          LedgerHeaderDocNo = LedgerHeaderTab.DocNo,
                                          LedgerHeaderDocDate = LedgerHeaderTab.DocDate,
                                          PartyDocNo = LedgerHeaderTab.PartyDocNo,
                                          PartyDocDate = LedgerHeaderTab.PartyDocDate ?? LedgerHeaderTab.DocDate,
                                          BillAmount = LedgerTab.AmtCr,
                                          AdjustedAmount = L.Amount
                                      }).ToList();
            }


            if (s.DrCr == NatureConstants.Credit)
            {
                LedgerAdjustedList = (from L in db.LedgerAdj
                                      join Ld in db.Ledger on L.DrLedgerId equals Ld.LedgerId into LedgerTable
                                      from LedgerTab in LedgerTable.DefaultIfEmpty()
                                      join H in db.LedgerHeader on LedgerTab.LedgerHeaderId equals H.LedgerHeaderId into LedgerHeaderTable
                                      from LedgerHeaderTab in LedgerHeaderTable.DefaultIfEmpty()
                                      where L.CrLedgerId == s.LedgerId
                                      select new PendingLedgerViewModel
                                      {
                                          LedgerAdjId = L.LedgerAdjId,
                                          LedgerId = LedgerTab.LedgerId,
                                          LedgerHeaderDocNo = LedgerHeaderTab.DocNo,
                                          LedgerHeaderDocDate = LedgerHeaderTab.DocDate,
                                          PartyDocNo = LedgerHeaderTab.PartyDocNo,
                                          PartyDocDate = LedgerHeaderTab.PartyDocDate ?? LedgerHeaderTab.DocDate,
                                          BillAmount = LedgerTab.AmtDr,
                                          AdjustedAmount = L.Amount
                                      }).ToList();
            }

            s.LedgerViewModel = LedgerAdjustedList;

            ViewBag.Name = new LedgerAccountService(_unitOfWork).Find(Line.LedgerAccountId).LedgerAccountName + "  " + "Amount : " + Line.Amount.ToString();


            if (s == null)
            {
                return HttpNotFound();
            }
            return PartialView("_LedgerAdj_Single", s);
        }

        public ActionResult GetLedgerIds_Adusted(string searchTerm, int pageSize, int pageNum, int? filter, string filter2, int filter3)//DocTypeId
        {
            var Query = new LedgerLineService(_unitOfWork).GetLedgerIds_Adusted(filter, filter2, filter3, searchTerm);
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

        public JsonResult GetLedgerIds_AdustedDetailJson(int LedgerId)
        {
            var temp = (from L in db.Ledger
                        join V in db.ViewLedgerBalance on L.LedgerId equals V.LedgerId into LedgerBalanceTable from LedgerBalanceTab in LedgerBalanceTable.DefaultIfEmpty()
                        join H in db.LedgerHeader on L.LedgerHeaderId equals H.LedgerHeaderId into LedgerHeaderTable from LedgerHeaderTab in LedgerHeaderTable.DefaultIfEmpty()
                        where L.LedgerId == LedgerId
                        select new PendingLedgerViewModel
                        {
                            LedgerHeaderDocNo = LedgerHeaderTab.DocNo,
                            LedgerHeaderDocDate = LedgerHeaderTab.DocDate,
                            PartyDocNo = LedgerHeaderTab.PartyDocNo,
                            PartyDocDate = LedgerHeaderTab.PartyDocDate ?? LedgerHeaderTab.DocDate,
                            BalanceAmount = LedgerBalanceTab.Balance,
                            BillAmount = L.AmtDr != 0 ? L.AmtDr : L.AmtCr,
                            LedgerAccountId = L.LedgerAccountId,
                            LedgerAccountName = L.LedgerAccount.LedgerAccountName
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp);
            }
            else
            {
                return null;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _LedgerAdj_Single(LedgerToAdjustViewModel_Single svm)
        {
            if (svm.BalanceAmount < svm.AdjustedAmount)
            {
                string message = "Adjusted Amount is geated then balance amount.";
                TempData["CSEXCL"] += message;
                return PartialView("_LedgerAdj_Single", svm);
            }

            if (svm.BalanceAmount_Adjusted < svm.AdjustedAmount)
            {
                string message = "Adjusted Amount is geated then selected bill balance amount.";
                TempData["CSEXCL"] += message;
                return PartialView("_LedgerAdj_Single", svm);
            }



            if (ModelState.IsValid)
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


                if (svm.AdjustedAmount != 0 && svm.AdjustedAmount != null)
                {
                    LedgerAdj Adj = new LedgerAdj();
                    if (svm.DrCr == NatureConstants.Debit)
                    {
                        Adj.DrLedgerId = svm.LedgerId;
                        Adj.CrLedgerId = svm.LedgerId_Adjusted;
                    }
                    else
                    {
                        Adj.CrLedgerId = svm.LedgerId;
                        Adj.DrLedgerId = svm.LedgerId_Adjusted;
                    }

                    Adj.Amount = svm.AdjustedAmount ?? 0;
                    Adj.SiteId = SiteId;
                    Adj.CreatedDate = DateTime.Now;
                    Adj.ModifiedDate = DateTime.Now;
                    Adj.CreatedBy = User.Identity.Name;
                    Adj.ModifiedBy = User.Identity.Name;
                    Adj.ObjectState = Model.ObjectState.Added;
                    db.LedgerAdj.Add(Adj);
                }

                try
                {
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_LedgerAdj_Single", svm);
                }

                //return Json(new { success = true });
                return RedirectToAction("_LedgerAdj_Single", new { id = svm.LedgerLineId });

            }

            return PartialView("_LedgerAdj", svm);
        }


        [HttpGet]
        public ActionResult _LedgerAdj_DeletePost(int id)//LedgerAdjId
        {
            LedgerAdj Adj = db.LedgerAdj.Find(id);
            Adj.ObjectState = Model.ObjectState.Deleted;
            db.LedgerAdj.Add(Adj);
            db.SaveChanges();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBalanceAmountJson(int LedgerHeaderId, string DrCr)
        {
            Decimal TotalAmountDr = 0;
            Decimal TotalAmountCr = 0;
            Decimal Amount = 0;

            if (db.LedgerLine.Where(m => m.DrCr == NatureConstants.Debit && m.LedgerHeaderId == LedgerHeaderId).Count() > 0)
            {
                TotalAmountDr = db.LedgerLine.Where(m => m.DrCr == NatureConstants.Debit && m.LedgerHeaderId == LedgerHeaderId).Sum(m => m.Amount);
            }

            if (db.LedgerLine.Where(m => m.DrCr == NatureConstants.Credit && m.LedgerHeaderId == LedgerHeaderId).Count() > 0)
            {
                TotalAmountCr = db.LedgerLine.Where(m => m.DrCr == NatureConstants.Credit && m.LedgerHeaderId == LedgerHeaderId).Sum(m => m.Amount);
            }


            if (TotalAmountCr > TotalAmountDr)
            {
                if (DrCr == NatureConstants.Debit)
                {
                    Amount = TotalAmountCr - TotalAmountDr;
                }
                else
                {
                    Amount = 0;
                }
            }
            if (TotalAmountDr > TotalAmountCr)
            {
                if (DrCr == NatureConstants.Credit)
                {
                    Amount = TotalAmountDr - TotalAmountCr;
                }
                else
                {
                    Amount = 0;
                }
            }

            return Json(Amount);
        }



    }

}
