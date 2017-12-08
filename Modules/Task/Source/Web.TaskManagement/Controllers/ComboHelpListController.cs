using System.Web.Mvc;
using Service;
using Presentation.Helper;


namespace Web
{
    [Authorize]
    public class ComboHelpListController : System.Web.Mvc.Controller
    {
        private IComboHelpListService cbl;
        public ComboHelpListController(IComboHelpListService _cbl)
        {
            cbl = _cbl;
        }

        public ActionResult GetUsers(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = cbl.GetUsers(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleUsers(string Ids)
        {
            return Json(cbl.GetUser(Ids));
        }

        public JsonResult SetUsers(string Ids)
        {
            return Json(cbl.GetMultipleUsers(Ids));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cbl.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}


