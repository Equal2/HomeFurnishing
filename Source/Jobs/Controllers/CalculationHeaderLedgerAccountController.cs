using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using System.Net.Http;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using Presentation.ViewModels;
using Model.ViewModels;
using AutoMapper;
using System.Configuration;
using Jobs.Helpers;
using Presentation;
using System.Text;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;
using Model.ViewModel;
using System.Data.Entity.Infrastructure;
using System.Xml.Linq;

namespace Jobs.Controllers
{

    [Authorize]
    public class CalculationHeaderLedgerAccountController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ICalculationHeaderLedgerAccountService _CalculationHeaderLedgerAccountService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public CalculationHeaderLedgerAccountController(ICalculationHeaderLedgerAccountService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _CalculationHeaderLedgerAccountService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        public ActionResult HeaderIndex()
        {
            var p = _CalculationHeaderLedgerAccountService.GetHeaderIndex();
            return View("HeaderIndex",p);
        }
        [HttpGet]
        public ActionResult Create()
        {
            CalculationHeaderLedgerAccountViewModel vm = new CalculationHeaderLedgerAccountViewModel();
            return View(vm);
        }

        [HttpPost]
        public ActionResult CreatePost(CalculationHeaderLedgerAccountViewModel vm)
        {
            if (vm.CalculationId <= 0)
                ModelState.AddModelError("CalculationId", "The Calculation field is required.");
            if (vm.DocTypeId <= 0)
                ModelState.AddModelError("DocTypeId", "The DocumentType field is required.");

            if (ModelState.IsValid)
            {
                return View("Create", vm).Success("Data saved successfully");
            }
            else
                return View("Create", vm);
        }

        [HttpGet]
        public JsonResult Index(int id,int DocTypeId)
        {
            var p = _CalculationHeaderLedgerAccountService.GetCalculationListForIndex(id,DocTypeId).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _Create(int CalculationId,int DocTypeId) //Id ==>Calculation Id
        {
            CalculationHeaderLedgerAccountViewModel vm = new CalculationHeaderLedgerAccountViewModel();
            vm.CalculationId = CalculationId;
            vm.DocTypeId = DocTypeId;
            return PartialView("_Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(CalculationHeaderLedgerAccountViewModel svm)
        {
            CalculationHeaderLedgerAccount s = Mapper.Map<CalculationHeaderLedgerAccountViewModel, CalculationHeaderLedgerAccount>(svm);                       

            if (ModelState.IsValid)
            {
                if (svm.CalculationHeaderLedgerAccountId <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                    s.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    s.ObjectState = Model.ObjectState.Added;

                    _CalculationHeaderLedgerAccountService.Create(s);
                   
                    try
                    {
                        _unitOfWork.Save();
                    }
                   
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);

                    }
                    return Json(new { success = true });
                    //return RedirectToAction("_Create", new { CalculationId = s.CalculationId, DocTypeId=s.DocTypeId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();  
                    CalculationHeaderLedgerAccount temp1 = _CalculationHeaderLedgerAccountService.Find(svm.CalculationHeaderLedgerAccountId);

                    CalculationHeaderLedgerAccount ExRec = Mapper.Map<CalculationHeaderLedgerAccount>(temp1);

                    temp1.CalculationFooterId = s.CalculationFooterId;
                    temp1.CostCenterId = s.CostCenterId;
                    temp1.LedgerAccountCrId = s.LedgerAccountCrId;
                    temp1.LedgerAccountDrId = s.LedgerAccountDrId;
                    temp1.ContraLedgerAccountId = s.ContraLedgerAccountId;

                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    _CalculationHeaderLedgerAccountService.Update(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
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
                        return PartialView("_Create", svm);
                    }

                   //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
                   //temp1.CalculationHeaderLedgerAccountId,
                   //null,
                   //(int)ActivityTypeContants.Modified,
                   //"",
                   //User.Identity.Name, "", Modifications);

                    return Json(new { success = true });

                }
            }            
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult Edit(int id,int DoctypeId)
        {
            CalculationHeaderLedgerAccountViewModel temp = _CalculationHeaderLedgerAccountService.GetCalculationHeaderLedgerAccount(id,DoctypeId);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("Create", temp);
        }

        [HttpGet]
        public ActionResult _Edit(int? CHeaderLedgerId, int DocTypeId, int CalculationFooterId, int CalculationId)
        {
            if (CHeaderLedgerId.HasValue && CHeaderLedgerId > 0 )
            {

                CalculationHeaderLedgerAccountViewModel temp = _CalculationHeaderLedgerAccountService.GetCalculationHeaderLedgerAccount(CHeaderLedgerId.Value);

                if (temp == null)
                {
                    return HttpNotFound();
                }
                return PartialView("_Create", temp);

            }
            else
            {
                CalculationHeaderLedgerAccountViewModel vm = new CalculationHeaderLedgerAccountViewModel();
                vm.DocTypeId = DocTypeId;
                vm.CalculationFooterId = CalculationFooterId;
                vm.CalculationId = CalculationId;
                return PartialView("_Create", vm);
            }
        }

        [HttpGet]
        public ActionResult Delete(int id,int DocTypeId)
        {
            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
                DocTypeId=DocTypeId,
            };
            return PartialView("_Reason", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                var CalculationHeaderLedger=_CalculationHeaderLedgerAccountService.Find(vm.id);
                var LineRecords = new CalculationLineLedgerAccountService(_unitOfWork).GetCalculationListForIndex(vm.id, vm.DocTypeId);
                var HeaderRecords = _CalculationHeaderLedgerAccountService.GetCalculationListForIndex(vm.id, vm.DocTypeId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = CalculationHeaderLedger,
                });

                foreach (var item in LineRecords)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    new CalculationLineLedgerAccountService(_unitOfWork).Delete(item.CalculationLineLedgerAccountId);
                }

                foreach (var item in HeaderRecords)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });
                    new CalculationHeaderLedgerAccountService(_unitOfWork).Delete(item.CalculationHeaderLedgerAccountId);
                }
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

                //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
                // CalculationHeaderLedger.CalculationHeaderLedgerAccountId,
                // null,
                // (int)ActivityTypeContants.Deleted,
                // vm.Reason,
                // User.Identity.Name,
                // "", Modifications);

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
                    DocId = CalculationHeaderLedger.CalculationHeaderLedgerAccountId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = "",
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(CalculationHeaderLedgerAccountViewModel vm)
        {
            CalculationHeaderLedgerAccount CalculationHeaderLedgerAccount = _CalculationHeaderLedgerAccountService.Find(vm.CalculationHeaderLedgerAccountId);
            _CalculationHeaderLedgerAccountService.Delete(CalculationHeaderLedgerAccount);        

            try
            {
                _unitOfWork.Save();
            }
          
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("_Create", vm);

            }
            return Json(new { success = true });
        }

        public JsonResult GetCalculationFooter(int id,string term)
        {
            return Json(_CalculationHeaderLedgerAccountService.GetProductFooters(id,term), JsonRequestBehavior.AllowGet);
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
