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
    public class PostingLayoutController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IUnitOfWork _unitOfWork;
        IReportLineService _ReportLineService;
        public PostingLayoutController(IUnitOfWork work,IReportLineService line)
          {
              _unitOfWork = work;
              _ReportLineService = line;
          }

          [HttpGet]
          public ActionResult PostingLayout(string name)
          {
              ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(name);
              List<ReportLine> lines = _ReportLineService.GetReportLineList(header.ReportHeaderId).ToList();

              Dictionary<int, string> DefaultValues = TempData["PostingLayoutDefaultValues"] as Dictionary<int, string>;
              TempData["PostingLayoutDefaultValues"] = DefaultValues;
              foreach(var item in lines)
              {
                  if (DefaultValues!=null && DefaultValues.ContainsKey(item.ReportLineId))
                  {
                      item.DefaultValue = DefaultValues[item.ReportLineId];
                  }
              }

              ReportMasterViewModel vm = new ReportMasterViewModel();

              vm.ReportHeader = header;
              vm.ReportLine = lines;
              vm.ReportHeaderId = header.ReportHeaderId;
              
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
