using System.Web.Mvc;
using Components.ExceptionHandlers;
using Components.Logging;

namespace Web
{
    public class ActivityLogController : System.Web.Mvc.Controller
    {
        IExceptionHandler _exception;
        public ActivityLogController(IExceptionHandler exec)
        {
            _exception = exec;
        }

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
