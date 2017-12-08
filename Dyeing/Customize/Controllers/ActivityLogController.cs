using Components.Logging;
using System.Web.Mvc;

namespace Customize.Controllers
{
    public class ActivityLogController : Controller
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
