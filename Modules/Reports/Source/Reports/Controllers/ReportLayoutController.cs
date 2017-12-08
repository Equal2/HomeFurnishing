using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Service;
using Models.Reports.ViewModels;
using Models.Reports.Models;

namespace Web
{
    [Authorize]
    public class ReportLayoutController : System.Web.Mvc.Controller
    {
        IReportLineService _ReportLineService;
        IReportHeaderService _ReportHeaderservice;
        public ReportLayoutController(IReportLineService line, IReportHeaderService ReportHeaderServ)
        {
            _ReportLineService = line;
            _ReportHeaderservice = ReportHeaderServ;
        }

        [HttpGet]
        public ActionResult ReportLayout(string name)
        {
            ReportHeader header = _ReportHeaderservice.GetReportHeaderByName(name);
            List<ReportLineViewModel> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();

            Dictionary<int, string> DefaultValues = TempData["ReportLayoutDefaultValues"] as Dictionary<int, string>;
            TempData["ReportLayoutDefaultValues"] = DefaultValues;
            foreach (var item in lines)
            {
                if (DefaultValues != null && DefaultValues.ContainsKey(item.ReportLineId))
                {
                    item.DefaultValue = DefaultValues[item.ReportLineId];
                }
            }

            ReportMasterViewModel vm = new ReportMasterViewModel();

            if (TempData["closeOnSelectOption"] != null)
                vm.closeOnSelect = (bool)TempData["closeOnSelectOption"];

            vm.ReportHeader = header;
            vm.ReportLine = lines;
            vm.ReportHeaderId = header.ReportHeaderId;

            return View(vm);
        }

        public JsonResult SetSelectOption(bool Checked)
        {
            TempData["closeOnSelectOption"] = Checked;
            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ReportHeaderservice.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
