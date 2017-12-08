using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Presentation;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using Model.ViewModel;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    
    [Authorize]
    public class PersonOpeningController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IPersonOpeningService _PersonOpeningService;
        IPersonService _PersonService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();

        public PersonOpeningController(IPersonOpeningService PersonOpening, IPersonService Person, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PersonOpeningService = PersonOpening;
            _PersonService = Person;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _PersonOpeningService.GetPersonOpeningListForIndex(id).ToList();                               
            return Json(p, JsonRequestBehavior.AllowGet);
            
        }


        public void PrepareViewBag()
        {
            List<SelectListItem> DrCr = new List<SelectListItem>();
            DrCr.Add(new SelectListItem { Text = NatureConstants.Debit, Value = NatureConstants.Debit });
            DrCr.Add(new SelectListItem { Text = NatureConstants.Credit, Value = NatureConstants.Credit });

            ViewBag.DrCrList = new SelectList(DrCr, "Value", "Text");

        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            PersonOpeningViewModel s = new PersonOpeningViewModel();
            s.PersonId = Id;

            s.DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Opening).DocumentTypeId;
            //s.DocDate = DateTime.Now;

            DateTime TodayDate = DateTime.Now;
            var temp = (from Be in db.BusinessSession where TodayDate >= Be.FromDate && TodayDate <= Be.UptoDate select Be).FirstOrDefault();
            if (temp != null)
            {
                s.DocDate = temp.FromDate.AddDays(-1);
            }
            else
            {
                s.DocDate = DateTime.Now;
            }


            s.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            s.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            s.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", s.DocTypeId, s.DocDate, s.DivisionId, s.SiteId);


            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "LedgerSetting", new { id = s.DocTypeId }).Warning("Please create Ledger settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            s.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);

            PrepareViewBag();
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PersonOpeningViewModel svm)
        {
            if (ModelState.IsValid)
            {
                LedgerAccount LedgerAccount = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(svm.PersonId);

                if (svm.LedgerHeaderId <= 0)
                {
                    var temp = (from H in db.LedgerHeader
                                where H.DocTypeId == svm.DocTypeId && H.DocNo == svm.DocNo && H.SiteId == svm.SiteId && H.DivisionId == svm.DivisionId
                                select H).FirstOrDefault();

                    if (temp != null)
                    {
                        if (svm.LedgerSetting.IsAutoDocNo == true)
                        {
                            svm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".LedgerHeaders", svm.DocTypeId, svm.DocDate, svm.DivisionId, svm.SiteId);
                        }
                    }
                }


                if(svm.LedgerHeaderId == 0)
                {
                    LedgerHeader Header = new LedgerHeader();

                    Header.DocHeaderId = svm.PersonId;
                    Header.DocNo = svm.DocNo;
                    Header.DocTypeId = svm.DocTypeId;
                    Header.DocDate = svm.DocDate;
                    Header.SiteId = svm.SiteId;
                    Header.DivisionId = svm.DivisionId;
                    Header.PaymentFor = null;
                    Header.AdjustmentType = null;
                    Header.ProcessId = null;
                    Header.GodownId = null;
                    Header.LedgerAccountId = null;
                    Header.DrCr = null;
                    Header.PartyDocNo = svm.PartyDocNo;
                    Header.PartyDocDate = svm.PartyDocDate;
                    Header.Narration = svm.Narration ;
                    Header.CreatedDate = DateTime.Now;
                    Header.CreatedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ObjectState = Model.ObjectState.Added;
                    db.LedgerHeader.Add(Header);


                    LedgerLine Line = new LedgerLine();

                    Line.LedgerHeaderId = Header.LedgerHeaderId;
                    Line.LedgerAccountId = LedgerAccount.LedgerAccountId;
                    Line.Amount = svm.Amount;
                    Line.ChqNo = null;
                    Line.ChqDate = null;
                    Line.CostCenterId = null;
                    Line.BaseRate = 0;
                    Line.BaseValue = 0;
                    Line.ReferenceId = null;
                    Line.ProductUidId = null;
                    Line.ReferenceDocTypeId = null;
                    Line.ReferenceDocId = null;
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedDate = DateTime.Now;
                    Line.CreatedBy = User.Identity.Name;
                    Line.Remark = svm.Narration;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ObjectState = Model.ObjectState.Added;
                    db.LedgerLine.Add(Line);


                    Ledger Ledger = new Ledger();

                    if (svm.DrCr == NatureConstants.Debit)
                    {
                        Ledger.AmtDr = svm.Amount;

                    }
                    else if (svm.DrCr == NatureConstants.Credit)
                    {
                        Ledger.AmtCr = svm.Amount;
                    }

                    Ledger.ChqNo = Line.ChqNo;
                    Ledger.ContraLedgerAccountId = null;
                    Ledger.CostCenterId = Line.CostCenterId;
                    Ledger.DueDate = null;
                    Ledger.LedgerAccountId = Line.LedgerAccountId;
                    Ledger.LedgerHeaderId = Line.LedgerHeaderId;
                    Ledger.LedgerLineId = Line.LedgerLineId;
                    Ledger.ProductUidId = Line.ProductUidId;
                    Ledger.Narration = Header.Narration;
                    Ledger.ObjectState = Model.ObjectState.Added;
                    Ledger.LedgerId = 1;
                    db.Ledger.Add(Ledger);
                   



                    try
                    {
                        db.SaveChanges();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        PrepareViewBag();
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);
                    }
                    return RedirectToAction("_Create", new { id = svm.PersonId });
                }
                else
                {
                    LedgerHeader Header = new LedgerHeaderService(_unitOfWork).Find(svm.LedgerHeaderId);
                    LedgerLine Line = new LedgerLineService(_unitOfWork).FindByLedgerHeader(svm.LedgerHeaderId).FirstOrDefault();
                    Ledger Ledger = new LedgerService(_unitOfWork).FindForLedgerHeader(svm.LedgerHeaderId).FirstOrDefault();


                    Header.SiteId = svm.SiteId;
                    Header.DivisionId = svm.DivisionId;
                    Header.DocNo = svm.DocNo;
                    Header.DocTypeId = svm.DocTypeId;
                    Header.DocDate = svm.DocDate;
                    Header.PartyDocNo = svm.PartyDocNo;
                    Header.PartyDocDate = svm.PartyDocDate;
                    Header.Narration = svm.Narration;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ObjectState = Model.ObjectState.Modified;
                    db.LedgerHeader.Add(Header);


                    Line.Amount = svm.Amount;
                    Line.ModifiedDate = DateTime.Now;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ObjectState = Model.ObjectState.Modified;
                    db.LedgerLine.Add(Line);


                    if (svm.DrCr == NatureConstants.Debit)
                    {
                        Ledger.AmtDr = svm.Amount;
                        Ledger.AmtCr = 0;

                    }
                    else if (svm.DrCr == NatureConstants.Credit)
                    {
                        Ledger.AmtCr = svm.Amount;
                        Ledger.AmtDr = 0;
                    }

                    Ledger.ChqNo = Line.ChqNo;
                    Ledger.CostCenterId = Line.CostCenterId;
                    Ledger.LedgerAccountId = Line.LedgerAccountId;
                    Ledger.LedgerHeaderId = Line.LedgerHeaderId;
                    Ledger.LedgerLineId = Line.LedgerLineId;
                    Ledger.ProductUidId = Line.ProductUidId;
                    Ledger.Narration = Header.Narration;
                    Ledger.ObjectState = Model.ObjectState.Modified;
                    db.Ledger.Add(Ledger);

                    try
                    {
                        db.SaveChanges();
                    }
                 
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        PrepareViewBag();
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);

                    }


                    return Json(new { success = true });

                }
            }

            PrepareViewBag();
            return PartialView("_Create",svm);
        }

        
        public ActionResult _Edit(int id)//LedgerHeaderId
        {
            PersonOpeningViewModel temp = _PersonOpeningService.GetPersonOpeningForEdit(id);

            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "LedgerSetting", new { id = temp.DocTypeId }).Warning("Please create Ledger settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            temp.LedgerSetting = Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);
            
            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return PartialView("_Create", temp);
        }


        [HttpGet]
        public ActionResult Delete(int id)//LedgerHeaderId
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonOpeningViewModel PersonOpening = _PersonOpeningService.GetPersonOpeningForEdit(id);
            if (PersonOpening == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag();
            return View(PersonOpening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PersonOpeningViewModel vm)
        {
            LedgerHeader Header = new LedgerHeaderService(_unitOfWork).Find(vm.LedgerHeaderId);
            LedgerLine Line = new LedgerLineService(_unitOfWork).FindByLedgerHeader(vm.LedgerHeaderId).FirstOrDefault();
            Ledger Ledger = new LedgerService(_unitOfWork).FindForLedgerHeader(vm.LedgerHeaderId).FirstOrDefault();

            Ledger LedgerToDelete = db.Ledger.Find(Ledger.LedgerId);
            LedgerLine LedgerLineToDelete = db.LedgerLine.Find(Line.LedgerLineId);
            LedgerHeader LedgerHeaderToDelete = db.LedgerHeader.Find(Header.LedgerHeaderId);

            LedgerToDelete.ObjectState = Model.ObjectState.Deleted;
            db.Ledger.Remove(LedgerToDelete);

            LedgerLineToDelete.ObjectState = Model.ObjectState.Deleted;
            db.LedgerLine.Remove(LedgerLineToDelete);

            LedgerHeaderToDelete.ObjectState = Model.ObjectState.Deleted;
            db.LedgerHeader.Remove(LedgerHeaderToDelete);

            try
            {
                db.SaveChanges();
            }
         
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                PrepareViewBag();
                ModelState.AddModelError("", message);
                return PartialView("EditSize", vm);

            }
            return Json(new { success = true });           
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
