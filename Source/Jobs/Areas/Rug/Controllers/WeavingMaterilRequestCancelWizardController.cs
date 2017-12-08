using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Data.Infrastructure;
using Core.Common;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using System.Configuration;


namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class WeavingMaterialRequestCancelWizardController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public WeavingMaterialRequestCancelWizardController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _exception = exec;
            _unitOfWork = unitOfWork;
        }


        public ActionResult RequestCancelWizard(int Id)
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            //Getting Settings
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(Id, DivisionId, SiteId);

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateRequisitionCancel", "RequisitionSetting", new { id = Id });
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            ViewBag.DocTypeId = Id;
            return View();
        }


        public JsonResult AjaxGetJsonRequestCancelData(DateTime FromDate, DateTime ? ToDate)
        {
            return Json(new { Success = true, Data = new RequisitionCancelHeaderService(_unitOfWork).GetPendingRequisitionsForcancel(FromDate, ToDate ?? DateTime.Now.Date).ToList() }, JsonRequestBehavior.AllowGet);
        }





        public ActionResult CancelRequisition(int HeaderId, int DocTypeId)
        {

            //var PersonId = new EmployeeService(_unitOfWork).GetEmloyeeForUser(System.Web.HttpContext.Current.User.Identity.GetUserId());
            bool Flag = false;

            Flag = new RequisitionCancelHeaderService(_unitOfWork).CancelPendingWeavingRequisitions(HeaderId, User.Identity.Name, DocTypeId);

            return Json(new { Success = Flag });
        }

        public JsonResult GetCancelDetail(int HeaderId)
        {

            var CancelDetail = (from p in db.ViewRequisitionBalance
                                where p.RequisitionHeaderId == HeaderId && p.BalanceQty > 0
                                join t2 in db.Product on p.ProductId equals t2.ProductId
                                join t3 in db.Dimension1 on p.Dimension1Id equals t3.Dimension1Id into table3
                                from tab3 in table3.DefaultIfEmpty()
                                select new
                                {
                                    p.BalanceQty,
                                    t2.ProductName,
                                    tab3.Dimension1Name,
                                }).ToList();

            return Json(new { Data = CancelDetail }, JsonRequestBehavior.AllowGet);

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
