using System.Web.Mvc;
using Presentation.Helper;
using Services.Reports;

namespace Web
{
    [Authorize]
    public class ComboHelpListController : Controller
    {
        private IReportHelpListService _comboHelpListService;

        public ComboHelpListController(IReportHelpListService combohelpListServ)
        {
            _comboHelpListService = combohelpListServ;
        }

        public ActionResult GetSelect2Data(string searchTerm, int pageSize, int pageNum, string SqlProcGet)
        {
            return new JsonpResult
            {
                Data = _comboHelpListService.GetSelect2HelpList(SqlProcGet, searchTerm, pageSize, pageNum),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSelct2Data(string Ids, string SqlProcSet)
        {
            return Json(_comboHelpListService.SetSelct2Data(Ids, SqlProcSet));
        }

        public JsonResult SetSingleSelect2Data(int Ids, string SqlProcSet)
        {
            return Json(_comboHelpListService.SetSingleSelect2Data(Ids, SqlProcSet));
        }

        public ActionResult SetDate(string Proc)
        {
            return Json(_comboHelpListService.SetDate(Proc), JsonRequestBehavior.AllowGet);
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
