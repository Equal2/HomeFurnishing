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
using Presentation.ViewModels;
using Presentation;
using Model.ViewModels;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class PackingReportLayoutController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IUnitOfWork _unitOfWork;
        IReportLineService _ReportLineService;
        public PackingReportLayoutController(IUnitOfWork work, IReportLineService line)
          {
              _unitOfWork = work;
              _ReportLineService = line;
          }

          [HttpGet]
          public ActionResult PackingReportLayout(string name, int Id = 0 )
          {
              ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(name);
              List<ReportLine> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();

              ReportMasterViewModel vm = new ReportMasterViewModel();

              vm.ReportHeader = header;
              vm.ReportLine = lines;
              vm.ReportHeaderId = header.ReportHeaderId;
              
              ViewBag.DataHeaderId = Id;
              return View(vm);
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
