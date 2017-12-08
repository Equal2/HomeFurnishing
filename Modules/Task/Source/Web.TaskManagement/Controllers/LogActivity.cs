using System;
using System.Web.Mvc;
using Service;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;

namespace Web
{
 

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




    public class ActivityLogForEditViewModel
    {
        public int DocId { get; set; }
        public int DocTypeId { get; set; }
        public string DocNo { get; set; }
        [Required, MinLength(20, ErrorMessage = "UserRemark must be a minimum of 20 characters")]
        public string UserRemark { get; set; }

    }
}
