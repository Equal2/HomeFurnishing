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
    public class CalculationLineLedgerAccountController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ICalculationLineLedgerAccountService _CalculationLineLedgerAccountService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public CalculationLineLedgerAccountController(ICalculationLineLedgerAccountService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _CalculationLineLedgerAccountService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }



        [HttpGet]
        public JsonResult Index(int id,int DocTypeId)
        {
            var p = _CalculationLineLedgerAccountService.GetCalculationListForIndex(id,DocTypeId).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Create(int CalculationId,int DocTypeId) //Id ==>Calculation Id
        {
            CalculationLineLedgerAccountViewModel vm = new CalculationLineLedgerAccountViewModel();
            vm.CalculationId = CalculationId;
            vm.DocTypeId = DocTypeId;
            return PartialView("_Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(CalculationLineLedgerAccountViewModel svm)
        {
            CalculationLineLedgerAccount s = Mapper.Map<CalculationLineLedgerAccountViewModel, CalculationLineLedgerAccount>(svm);                       

            if (ModelState.IsValid)
            {
                if (svm.CalculationLineLedgerAccountId <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                    s.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    s.ObjectState = Model.ObjectState.Added;

                    _CalculationLineLedgerAccountService.Create(s);
                   
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
                    CalculationLineLedgerAccount temp1 = _CalculationLineLedgerAccountService.Find(svm.CalculationLineLedgerAccountId);


                    CalculationLineLedgerAccount ExRec = Mapper.Map<CalculationLineLedgerAccount>(temp1);

                    temp1.CalculationProductId = s.CalculationProductId;
                    temp1.CostCenterId = s.CostCenterId;
                    temp1.LedgerAccountCrId = s.LedgerAccountCrId;
                    temp1.LedgerAccountDrId = s.LedgerAccountDrId;
                    temp1.ContraLedgerAccountId = s.ContraLedgerAccountId;

                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    _CalculationLineLedgerAccountService.Update(temp1);

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

                   // LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
                   //temp1.CalculationLineLedgerAccountId,
                   //null,
                   //(int)ActivityTypeContants.Modified,
                   //"",
                   //User.Identity.Name, "", Modifications);


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
                        DocId = temp1.CalculationLineLedgerAccountId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));



                    return Json(new { success = true });

                }
            }            
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int? CLineLedgerId, int DocTypeId, int CalculationProductId, int CalculationId)
        {
            if (CLineLedgerId.HasValue && CLineLedgerId > 0)
            {

                CalculationLineLedgerAccountViewModel temp = _CalculationLineLedgerAccountService.GetCalculationLineLedgerAccount(CLineLedgerId.Value);

                if (temp == null)
                {
                    return HttpNotFound();
                }
                return PartialView("_Create", temp);

            }
            else
            {
                CalculationLineLedgerAccountViewModel vm = new CalculationLineLedgerAccountViewModel();
                vm.DocTypeId = DocTypeId;
                vm.CalculationProductId = CalculationProductId;
                vm.CalculationId = CalculationId;
                return PartialView("_Create", vm);
            }            
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(CalculationLineLedgerAccountViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            CalculationLineLedgerAccount CalculationLineLedgerAccount = _CalculationLineLedgerAccountService.Find(vm.CalculationLineLedgerAccountId);
            _CalculationLineLedgerAccountService.Delete(CalculationLineLedgerAccount);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = CalculationLineLedgerAccount,
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
                return PartialView("_Create", vm);
            }

            //LogActivity.LogActivityDetail(new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
            //     CalculationLineLedgerAccount.CalculationLineLedgerAccountId,
            //     null,
            //     (int)ActivityTypeContants.Deleted,
            //     "",
            //     User.Identity.Name,
            //     "", Modifications);


            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.CalculationLedgerAccount).DocumentTypeId,
                DocId = CalculationLineLedgerAccount.CalculationLineLedgerAccountId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                UserRemark = "",
                xEModifications = Modifications,
            }));

            return Json(new { success = true });
        }

        public JsonResult GetCalculationProduct(int id, string term)
        {
            return Json(_CalculationLineLedgerAccountService.GetCalculationProducts(id, term), JsonRequestBehavior.AllowGet);
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
