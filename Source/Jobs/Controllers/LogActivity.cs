using System;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Model.ViewModel;

namespace Jobs.Controllers
{
    [Authorize]
    public class LogActivity
    {
        public static void LogActivityDetail(ActiivtyLogViewModel lvm)
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            ActivityLog log = new ActivityLog()
            {
                DocTypeId = lvm.DocTypeId,
                DocLineId = lvm.DocLineId,
                ActivityType = lvm.ActivityType,
                CreatedBy = lvm.User,
                UserRemark = lvm.UserRemark,
                Modifications = lvm.xEModifications != null ? lvm.xEModifications.ToString() : "",
                CreatedDate = DateTime.Now,
                DocNo = lvm.DocNo,
                DocId = lvm.DocId,
                DocDate=lvm.DocDate,
                Narration=lvm.Narration,
                ControllerName=lvm.ControllerName,
                ActionName=lvm.ActionName,
                DocStatus=lvm.DocStatus,
                SiteId=SiteId,
                DivisionId=DivisionId,
                ObjectState=Model.ObjectState.Added,
            };

            using (LogApplicationDbContext context = new LogApplicationDbContext())
            {
                context.ActivityLog.Add(log);
                context.SaveChanges();
            }
            

        }      

    }

    public class ActivityLogController : System.Web.Mvc.Controller
    {
        public ActionResult LogEditReason()
        {            
            return PartialView("~/Views/Shared/_Reason.cshtml");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostLogReason(ActivityLogForEditViewModel vm)
        {
            if(ModelState.IsValid)
            {            
                return Json(new { success = true,UserRemark=vm.UserRemark });
            }
            return PartialView("~/Views/Shared/_Reason.cshtml", vm);
        }
    }
}
