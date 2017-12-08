using System.Web.Mvc;
using Presentation.Helper;
using Service;
using Services.BasicSetup;


namespace Web
{
    [Authorize]
    public class ComboHelpListController : System.Web.Mvc.Controller
    {
        private readonly ISiteService _siteService;
        private readonly IDivisionService _divisionService;
        private readonly IGodownService _godownService;
        private readonly IComboHelpListService _comboHelpListService;

        public ComboHelpListController(ISiteService siteServ, IDivisionService divserv,
            IGodownService godownServ, IComboHelpListService comboHelpListServ)
        {
            _siteService = siteServ;
            _divisionService = divserv;
            _godownService = godownServ;
            _comboHelpListService = comboHelpListServ;
        }

        public ActionResult GetUsers(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _comboHelpListService.GetUsersById(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleUsers(string Ids)
        {
            return Json(_comboHelpListService.GetUserById(Ids));
        }

        public JsonResult SetUsers(string Ids)
        {
            return Json(_comboHelpListService.GetMultipleUsersById(Ids));
        }

        public ActionResult GetSite(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _siteService.GetList(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleSite(int Ids)
        {
            return Json(_siteService.GetValue(Ids));
        }

        public JsonResult SetSite(string Ids)
        {
            return Json(_siteService.GetListCsv(Ids));
        }

        public ActionResult GetDivision(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _divisionService.GetList(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleDivision(int Ids)
        {
            return Json(_divisionService.GetValue(Ids));
        }

        public JsonResult SetDivision(string Ids)
        {
            return Json(_divisionService.GetListCsv(Ids));
        }

        public ActionResult GetGodown(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _godownService.GetList(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleGodown(int Ids)
        {
            return Json(_godownService.GetValue(Ids));
        }

        public JsonResult SetGodown(string Ids)
        {
            return Json(_godownService.GetListCsv(Ids));
        }

        public ActionResult GetRoles(string searchTerm, int pageSize, int pageNum)
        {
            return new JsonpResult
            {
                Data = _comboHelpListService.GetRoles(searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleRoles(string Ids)
        {
            return Json(_comboHelpListService.GetRole(Ids));
        }

        public JsonResult SetRoles(string Ids)
        {
            return Json(_comboHelpListService.GetMultipleRoles(Ids));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _comboHelpListService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}